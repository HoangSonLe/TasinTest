using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// Controller for managing purchase orders
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class PurchaseOrderController : BaseController<PurchaseOrderController>
    {
        private IPurchaseOrderService _purchaseOrderService;
        private IInvoiceService _invoiceService;
        private ICommonService _commonService;
        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            IInvoiceService invoiceService,
            ICommonService commonService,
            IUserService userService,
            ILogger<PurchaseOrderController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _purchaseOrderService = purchaseOrderService;
            _invoiceService = invoiceService;
            _commonService = commonService;
        }

        [HttpGet]
        [Route("PurchaseOrder/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of purchase orders with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of purchase orders</returns>
        /// <response code="200">Returns the list of purchase orders</response>
        [HttpGet]
        [Route("PurchaseOrder/GetPurchaseOrderList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<PurchaseOrderViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public async Task<IActionResult> GetPurchaseOrderList([FromQuery] PurchaseOrderSearchModel searchModel)
        {
            var result = await _purchaseOrderService.GetPurchaseOrderList(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get a purchase order by ID
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Purchase order details</returns>
        /// <response code="200">Returns the purchase order details</response>
        [HttpGet]
        [Route("PurchaseOrder/GetPurchaseOrderById")]
        [ProducesResponseType(typeof(Acknowledgement<PurchaseOrderViewModel>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public async Task<IActionResult> GetPurchaseOrderById(int purchaseOrderId)
        {
            var result = await _purchaseOrderService.GetPurchaseOrderById(purchaseOrderId);
            return Json(result);
        }

        /// <summary>
        /// Create a new purchase order
        /// </summary>
        /// <param name="postData">Purchase order data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPost]
        [Route("PurchaseOrder/CreatePurchaseOrder")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_PURCHASE_ORDER])]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderViewModel postData)
        {
            postData.Id = 0; // Ensure we're creating a new record
            var result = await _purchaseOrderService.CreateOrUpdatePurchaseOrder(postData);
            return Json(result);
        }

        /// <summary>
        /// Update an existing purchase order
        /// </summary>
        /// <param name="postData">Purchase order data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPut]
        [Route("PurchaseOrder/UpdatePurchaseOrder")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_PURCHASE_ORDER])]
        public async Task<IActionResult> UpdatePurchaseOrder([FromBody] PurchaseOrderViewModel postData)
        {
            if (postData.Id <= 0)
            {
                return Json(new Acknowledgement { IsSuccess = false, ErrorMessageList = new List<string> { "ID không hợp lệ" } });
            }

            var result = await _purchaseOrderService.CreateOrUpdatePurchaseOrder(postData);
            return Json(result);
        }

        /// <summary>
        /// Delete a purchase order by ID
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpDelete]
        [Route("PurchaseOrder/DeletePurchaseOrderById/{purchaseOrderId}")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_PURCHASE_ORDER])]
        public async Task<Acknowledgement> DeletePurchaseOrderById([FromRoute] int purchaseOrderId)
        {
            return await _purchaseOrderService.DeletePurchaseOrderById(purchaseOrderId);
        }

        /// <summary>
        /// Generate invoice from purchase order
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Invoice data</returns>
        /// <response code="200">Returns the invoice data</response>
        [HttpGet]
        [Route("PurchaseOrder/GenerateInvoice")]
        [ProducesResponseType(typeof(Acknowledgement<InvoiceViewModel>), 200)]
        public async Task<IActionResult> GenerateInvoice(int purchaseOrderId)
        {
            try
            {
                var result = await _invoiceService.GenerateInvoiceFromPurchaseOrder(purchaseOrderId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Logger.LogError($"GenerateInvoice: {ex.Message}");
                return Json(new Acknowledgement<InvoiceViewModel>
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Export purchase order invoice as PDF
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>PDF file</returns>
        /// <response code="200">Returns the PDF file</response>
        [HttpGet]
        [Route("PurchaseOrder/ExportInvoicePdf")]
        public async Task<IActionResult> ExportInvoicePdf(int purchaseOrderId)
        {
            try
            {
                var invoiceResult = await _invoiceService.GenerateInvoiceFromPurchaseOrder(purchaseOrderId);
                if (!invoiceResult.IsSuccess || invoiceResult.Data == null)
                {
                    return BadRequest(new { message = "Không thể tạo hóa đơn từ Purchase Order này" });
                }

                var pdfBytes = await _invoiceService.ExportInvoiceAsPdf(invoiceResult.Data);
                var fileName = $"Invoice_{invoiceResult.Data.InvoiceCode}_{DateTime.Now:yyyyMMdd_HHmmss}.html";

                // For now, return HTML for browser printing. In production, use proper PDF library
                Response.Headers.Add("Content-Disposition", PdfHelper.GetContentDispositionHeader(fileName, true));
                return File(pdfBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError($"ExportInvoicePdf: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Export purchase order invoice as Excel
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Excel file</returns>
        /// <response code="200">Returns the Excel file</response>
        [HttpGet]
        [Route("PurchaseOrder/ExportInvoiceExcel")]
        public async Task<IActionResult> ExportInvoiceExcel(int purchaseOrderId)
        {
            try
            {
                var invoiceResult = await _invoiceService.GenerateInvoiceFromPurchaseOrder(purchaseOrderId);
                if (!invoiceResult.IsSuccess || invoiceResult.Data == null)
                {
                    return BadRequest(new { message = "Không thể tạo hóa đơn từ Purchase Order này" });
                }

                var excelBytes = await _invoiceService.ExportInvoiceAsExcel(invoiceResult.Data);
                var fileName = $"Invoice_{invoiceResult.Data.InvoiceCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                Response.Headers.Add("Content-Disposition", ExcelHelper.GetContentDispositionHeader(fileName));
                return File(excelBytes, ExcelHelper.GetExcelContentType(), fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError($"ExportInvoiceExcel: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Export purchase order invoice as Word document
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Word document file</returns>
        /// <response code="200">Returns the Word document file</response>
        [HttpGet]
        [Route("PurchaseOrder/ExportInvoiceWord")]
        public async Task<IActionResult> ExportInvoiceWord(int purchaseOrderId)
        {
            try
            {
                var invoiceResult = await _invoiceService.GenerateInvoiceFromPurchaseOrder(purchaseOrderId);
                if (!invoiceResult.IsSuccess || invoiceResult.Data == null)
                {
                    return BadRequest(new { message = "Không thể tạo hóa đơn từ Purchase Order này" });
                }

                var wordBytes = await _invoiceService.ExportInvoiceAsWord(invoiceResult.Data);
                var fileName = $"Invoice_{invoiceResult.Data.InvoiceCode}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                return File(wordBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError($"ExportInvoiceWord: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Preview invoice HTML in browser
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>HTML content for preview</returns>
        /// <response code="200">Returns the HTML content</response>
        [HttpGet]
        [Route("PurchaseOrder/PreviewInvoice")]
        public async Task<IActionResult> PreviewInvoice(int purchaseOrderId)
        {
            try
            {
                var invoiceResult = await _invoiceService.GenerateInvoiceFromPurchaseOrder(purchaseOrderId);
                if (!invoiceResult.IsSuccess || invoiceResult.Data == null)
                {
                    return BadRequest(new { message = "Không thể tạo hóa đơn từ Purchase Order này" });
                }

                var html = await _invoiceService.GenerateInvoiceHtml(invoiceResult.Data);

                return Content(html, "text/html", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.LogError($"PreviewInvoice: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

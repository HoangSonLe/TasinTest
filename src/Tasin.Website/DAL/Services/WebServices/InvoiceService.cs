using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebServices
{
    /// <summary>
    /// Service for invoice operations
    /// </summary>
    public class InvoiceService : BaseService<InvoiceService>, IInvoiceService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public InvoiceService(
            IPurchaseOrderRepository purchaseOrderRepository,
            IPurchaseOrderItemRepository purchaseOrderItemRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            IMapper mapper,
            ILogger<InvoiceService> logger,
            SampleDBContext dbContext,
            IUserRepository userRepository,
            IConfiguration configuration,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Acknowledgement<InvoiceViewModel>> GenerateInvoiceFromPurchaseOrder(int purchaseOrderId)
        {
            var response = new Acknowledgement<InvoiceViewModel>();
            try
            {
                // Get purchase order with customer information
                var purchaseOrder = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                    filter: po => po.ID == purchaseOrderId,
                    includeProperties: "Customer"
                );

                var po = purchaseOrder.FirstOrDefault();
                if (po == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessageList.Add("Purchase Order không tồn tại");
                    return response;
                }

                // Get purchase order items with product and unit information
                var poItems = await _purchaseOrderItemRepository.ReadOnlyRespository.GetAsync(
                    filter: poi => poi.PO_ID == purchaseOrderId,
                    includeProperties: "Product,Unit"
                );

                // Generate invoice code
                var invoiceCode = await Generator.GenerateEntityCodeAsync(EntityPrefix.Invoice, DbContext);

                // Create invoice view model
                var invoice = new InvoiceViewModel
                {
                    InvoiceCode = invoiceCode,
                    PurchaseOrderId = po.ID,
                    PurchaseOrderCode = po.Code,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30), // Default 30 days payment term
                    Customer = new CustomerInvoiceInfo
                    {
                        Id = po.Customer?.ID ?? 0,
                        Code = po.Customer?.Code ?? "",
                        Name = po.Customer?.Name ?? "",
                        Address = po.Customer?.Address ?? "",
                        PhoneContact = po.Customer?.PhoneContact ?? "",
                        Email = po.Customer?.Email ?? "",
                        TaxCode = po.Customer?.TaxCode ?? ""
                    },
                    Company = new CompanyInvoiceInfo(), // Uses default values
                    PaymentTerms = "Thanh toán trong vòng 30 ngày kể từ ngày xuất hóa đơn"
                };

                // Convert purchase order items to invoice items
                var invoiceItems = new List<InvoiceItemViewModel>();
                int sequenceNumber = 1;

                foreach (var item in poItems)
                {
                    var unitPrice = item.Price ?? 0;
                    var quantity = item.Quantity;
                    var taxRate = item.TaxRate ?? 0;

                    var amountBeforeTax = unitPrice * quantity;
                    var taxAmount = amountBeforeTax * taxRate / 100;
                    var totalAmount = amountBeforeTax + taxAmount;

                    var invoiceItem = new InvoiceItemViewModel
                    {
                        SequenceNumber = sequenceNumber++,
                        ProductId = item.Product_ID,
                        ProductCode = item.Product?.Code ?? "",
                        ProductName = item.Product?.Name ?? "",
                        Unit = item.Unit?.Name ?? "",
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TaxRate = taxRate,
                        AmountBeforeTax = amountBeforeTax,
                        TaxAmount = taxAmount,
                        TotalAmount = totalAmount,
                        Notes = item.Note ?? ""
                    };

                    invoiceItems.Add(invoiceItem);
                }

                invoice.Items = invoiceItems;

                // Calculate totals
                invoice.Subtotal = invoiceItems.Sum(i => i.AmountBeforeTax);
                invoice.TaxAmount = invoiceItems.Sum(i => i.TaxAmount);
                invoice.TotalAmount = invoiceItems.Sum(i => i.TotalAmount);

                response.Data = invoice;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GenerateInvoiceFromPurchaseOrder: {ex.Message}");
                return response;
            }
        }

        public async Task<byte[]> ExportInvoiceAsPdf(InvoiceViewModel invoiceData)
        {
            try
            {
                var html = await GenerateInvoiceHtml(invoiceData);
                var printableHtml = PdfHelper.ConvertHtmlToPrintableFormat(html, $"Hóa đơn {invoiceData.InvoiceCode}");

                // For now, return HTML as bytes for browser printing
                // In production, use a proper PDF library like DinkToPdf or PuppeteerSharp
                return Encoding.UTF8.GetBytes(printableHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExportInvoiceAsPdf: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> ExportInvoiceAsExcel(InvoiceViewModel invoiceData)
        {
            try
            {
                using (var workbook = ExcelHelper.CreateWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Hóa đơn");

                    // Set up the invoice header
                    int row = 1;

                    // Company info
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), invoiceData.Company.Name);
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontSize = 16;
                    row += 2;

                    // Invoice title
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "HÓA ĐƠN BÁN HÀNG");
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontSize = 18;
                    worksheet.Range(row, 1, row, 8).Merge();
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    row += 2;

                    // Invoice details
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "Số hóa đơn:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 2), invoiceData.InvoiceCode);
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 4), "Ngày:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 5), PdfHelper.FormatDate(invoiceData.InvoiceDate));
                    row++;

                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "Mã đơn hàng:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 2), invoiceData.PurchaseOrderCode);
                    row += 2;

                    // Customer info
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "Thông tin khách hàng:");
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    row++;
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "Tên:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 2), invoiceData.Customer.Name);
                    row++;
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "Địa chỉ:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 2), invoiceData.Customer.Address);
                    row++;
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 1), "Mã số thuế:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 2), invoiceData.Customer.TaxCode);
                    row += 2;

                    // Items table header
                    var headers = new[] { "STT", "Mã SP", "Tên sản phẩm", "ĐVT", "Số lượng", "Đơn giá", "Thuế (%)", "Thành tiền" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ExcelHelper.SetCellValue(worksheet.Cell(row, i + 1), headers[i]);
                        worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    }
                    row++;

                    // Items
                    foreach (var item in invoiceData.Items)
                    {
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 1), item.SequenceNumber.ToString());
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 2), item.ProductCode);
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 3), item.ProductName);
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 4), item.Unit);
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 5), item.Quantity.ToString());
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 6), item.UnitPrice.ToString());
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 7), item.TaxRate.ToString());
                        ExcelHelper.SetCellValue(worksheet.Cell(row, 8), item.TotalAmount.ToString());

                        for (int i = 1; i <= 8; i++)
                        {
                            worksheet.Cell(row, i).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        }
                        row++;
                    }

                    // Totals
                    row++;
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 6), "Tổng tiền chưa thuế:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 8), invoiceData.Subtotal.ToString());
                    worksheet.Cell(row, 6).Style.Font.Bold = true;
                    row++;
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 6), "Tiền thuế:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 8), invoiceData.TaxAmount.ToString());
                    worksheet.Cell(row, 6).Style.Font.Bold = true;
                    row++;
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 6), "Tổng cộng:");
                    ExcelHelper.SetCellValue(worksheet.Cell(row, 8), invoiceData.TotalAmount.ToString());
                    worksheet.Cell(row, 6).Style.Font.Bold = true;
                    worksheet.Cell(row, 8).Style.Font.Bold = true;

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    return ExcelHelper.SaveToByteArray(workbook);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExportInvoiceAsExcel: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> ExportInvoiceAsWord(InvoiceViewModel invoiceData)
        {
            try
            {
                var html = await GenerateInvoiceHtml(invoiceData);

                // Create Word-compatible HTML
                var wordHtml = new StringBuilder();
                wordHtml.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
                wordHtml.AppendLine("<head><meta charset='utf-8'><title>Hóa đơn</title></head>");
                wordHtml.AppendLine("<body>");
                wordHtml.AppendLine(html);
                wordHtml.AppendLine("</body></html>");

                return Encoding.UTF8.GetBytes(wordHtml.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExportInvoiceAsWord: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GenerateInvoiceHtml(InvoiceViewModel invoiceData)
        {
            try
            {
                // Get template path
                var templatePath = TemplateHelper.GetTemplatePath("invoice_template_dynamic.html");

                // Check if template exists
                if (!TemplateHelper.TemplateExists(templatePath))
                {
                    _logger.LogWarning($"Template not found at {templatePath}, using fallback method");
                    return await GenerateInvoiceHtmlFallback(invoiceData);
                }

                // Process template with data
                var html = await TemplateHelper.ProcessInvoiceTemplate(templatePath, invoiceData);

                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GenerateInvoiceHtml: {ex.Message}");
                // Fallback to original method if template processing fails
                return await GenerateInvoiceHtmlFallback(invoiceData);
            }
        }

        /// <summary>
        /// Fallback method for generating invoice HTML when template is not available
        /// </summary>
        private async Task<string> GenerateInvoiceHtmlFallback(InvoiceViewModel invoiceData)
        {
            var html = new StringBuilder();

            html.AppendLine("<div class='invoice-container'>");

            // Header
            html.AppendLine("<div class='invoice-header'>");
            html.AppendLine("<div class='company-info'>");
            html.AppendLine($"<h2>{invoiceData.Company.Name}</h2>");
            html.AppendLine($"<p>{invoiceData.Company.Address}</p>");
            html.AppendLine($"<p>ĐT: {invoiceData.Company.Phone} | Email: {invoiceData.Company.Email}</p>");
            html.AppendLine($"<p>MST: {invoiceData.Company.TaxCode}</p>");
            html.AppendLine("</div>");
            html.AppendLine("<h1 class='invoice-title'>HÓA ĐƠN BÁN HÀNG</h1>");
            html.AppendLine("</div>");

            // Invoice and customer details
            html.AppendLine("<div class='invoice-details'>");
            html.AppendLine("<div class='invoice-info'>");
            html.AppendLine("<div class='info-label'>Thông tin hóa đơn:</div>");
            html.AppendLine($"<p><strong>Số hóa đơn:</strong> {invoiceData.InvoiceCode}</p>");
            html.AppendLine($"<p><strong>Ngày:</strong> {PdfHelper.FormatDate(invoiceData.InvoiceDate)}</p>");
            html.AppendLine($"<p><strong>Mã đơn hàng:</strong> {invoiceData.PurchaseOrderCode}</p>");
            if (invoiceData.DueDate.HasValue)
            {
                html.AppendLine($"<p><strong>Hạn thanh toán:</strong> {PdfHelper.FormatDate(invoiceData.DueDate.Value)}</p>");
            }
            html.AppendLine("</div>");

            html.AppendLine("<div class='customer-info'>");
            html.AppendLine("<div class='info-label'>Thông tin khách hàng:</div>");
            html.AppendLine($"<p><strong>Tên:</strong> {invoiceData.Customer.Name}</p>");
            html.AppendLine($"<p><strong>Mã:</strong> {invoiceData.Customer.Code}</p>");
            html.AppendLine($"<p><strong>Địa chỉ:</strong> {invoiceData.Customer.Address}</p>");
            html.AppendLine($"<p><strong>ĐT:</strong> {invoiceData.Customer.PhoneContact}</p>");
            html.AppendLine($"<p><strong>Email:</strong> {invoiceData.Customer.Email}</p>");
            html.AppendLine($"<p><strong>MST:</strong> {invoiceData.Customer.TaxCode}</p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Items table
            html.AppendLine("<table class='invoice-table'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>STT</th>");
            html.AppendLine("<th>Mã SP</th>");
            html.AppendLine("<th>Tên sản phẩm</th>");
            html.AppendLine("<th>ĐVT</th>");
            html.AppendLine("<th>Số lượng</th>");
            html.AppendLine("<th>Đơn giá</th>");
            html.AppendLine("<th>Thuế (%)</th>");
            html.AppendLine("<th>Thành tiền</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var item in invoiceData.Items)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td class='text-center'>{item.SequenceNumber}</td>");
                html.AppendLine($"<td>{item.ProductCode}</td>");
                html.AppendLine($"<td>{item.ProductName}</td>");
                html.AppendLine($"<td class='text-center'>{item.Unit}</td>");
                html.AppendLine($"<td class='text-right'>{item.Quantity:N0}</td>");
                html.AppendLine($"<td class='text-right'>{PdfHelper.FormatCurrency(item.UnitPrice)}</td>");
                html.AppendLine($"<td class='text-center'>{item.TaxRate:N1}%</td>");
                html.AppendLine($"<td class='text-right'>{PdfHelper.FormatCurrency(item.TotalAmount)}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            // Summary
            html.AppendLine("<div class='invoice-summary'>");
            html.AppendLine("<div class='summary-row'>");
            html.AppendLine("<span>Tổng tiền chưa thuế:</span>");
            html.AppendLine($"<span>{PdfHelper.FormatCurrency(invoiceData.Subtotal)}</span>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='summary-row'>");
            html.AppendLine("<span>Tiền thuế:</span>");
            html.AppendLine($"<span>{PdfHelper.FormatCurrency(invoiceData.TaxAmount)}</span>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='summary-row total'>");
            html.AppendLine("<span>Tổng cộng:</span>");
            html.AppendLine($"<span>{PdfHelper.FormatCurrency(invoiceData.TotalAmount)}</span>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Footer
            html.AppendLine("<div class='invoice-footer'>");
            if (!string.IsNullOrEmpty(invoiceData.PaymentTerms))
            {
                html.AppendLine($"<p><strong>Điều khoản thanh toán:</strong> {invoiceData.PaymentTerms}</p>");
            }
            if (!string.IsNullOrEmpty(invoiceData.Notes))
            {
                html.AppendLine($"<p><strong>Ghi chú:</strong> {invoiceData.Notes}</p>");
            }

            html.AppendLine("<div class='signature-section'>");
            html.AppendLine("<div class='signature-box'>");
            html.AppendLine("<p><strong>Người mua hàng</strong></p>");
            html.AppendLine("<div class='signature-line'>(Ký, ghi rõ họ tên)</div>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='signature-box'>");
            html.AppendLine("<p><strong>Người bán hàng</strong></p>");
            html.AppendLine("<div class='signature-line'>(Ký, ghi rõ họ tên)</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine("</div>");

            return html.ToString();
        }
    }
}

using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.Interfaces
{
    /// <summary>
    /// Interface for invoice service operations
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Generate invoice from purchase order
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Invoice view model</returns>
        Task<Acknowledgement<InvoiceViewModel>> GenerateInvoiceFromPurchaseOrder(int purchaseOrderId);

        /// <summary>
        /// Export invoice as PDF
        /// </summary>
        /// <param name="invoiceData">Invoice data</param>
        /// <returns>PDF file as byte array</returns>
        Task<byte[]> ExportInvoiceAsPdf(InvoiceViewModel invoiceData);

        /// <summary>
        /// Export invoice as Excel
        /// </summary>
        /// <param name="invoiceData">Invoice data</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> ExportInvoiceAsExcel(InvoiceViewModel invoiceData);

        /// <summary>
        /// Export invoice as Word document
        /// </summary>
        /// <param name="invoiceData">Invoice data</param>
        /// <returns>Word document as byte array</returns>
        Task<byte[]> ExportInvoiceAsWord(InvoiceViewModel invoiceData);

        /// <summary>
        /// Generate invoice HTML template
        /// </summary>
        /// <param name="invoiceData">Invoice data</param>
        /// <returns>HTML string</returns>
        Task<string> GenerateInvoiceHtml(InvoiceViewModel invoiceData);
    }
}

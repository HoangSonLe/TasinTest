using System.Text;

namespace Tasin.Website.Common.Helper
{
    /// <summary>
    /// Helper class for PDF operations
    /// </summary>
    public static class PdfHelper
    {
        /// <summary>
        /// Convert HTML to PDF using browser printing
        /// This is a simple implementation that generates HTML for browser printing
        /// For production use, consider using libraries like DinkToPdf, iTextSharp, or PuppeteerSharp
        /// </summary>
        /// <param name="html">HTML content</param>
        /// <param name="title">Document title</param>
        /// <returns>HTML formatted for PDF printing</returns>
        public static string ConvertHtmlToPrintableFormat(string html, string title = "Document")
        {
            var printableHtml = new StringBuilder();
            
            printableHtml.AppendLine("<!DOCTYPE html>");
            printableHtml.AppendLine("<html>");
            printableHtml.AppendLine("<head>");
            printableHtml.AppendLine("<meta charset='utf-8'>");
            printableHtml.AppendLine($"<title>{title}</title>");
            printableHtml.AppendLine("<style>");
            printableHtml.AppendLine(@"
                @media print {
                    body { margin: 0; }
                    .no-print { display: none !important; }
                }
                body {
                    font-family: 'Times New Roman', serif;
                    font-size: 12pt;
                    line-height: 1.4;
                    color: #000;
                    background: #fff;
                }
                .invoice-container {
                    max-width: 210mm;
                    margin: 0 auto;
                    padding: 20mm;
                    background: white;
                }
                .invoice-header {
                    text-align: center;
                    margin-bottom: 30px;
                    border-bottom: 2px solid #000;
                    padding-bottom: 20px;
                }
                .company-info {
                    text-align: center;
                    margin-bottom: 20px;
                }
                .invoice-title {
                    font-size: 24pt;
                    font-weight: bold;
                    margin: 20px 0;
                    text-transform: uppercase;
                }
                .invoice-details {
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 30px;
                }
                .customer-info, .invoice-info {
                    width: 48%;
                }
                .info-label {
                    font-weight: bold;
                    margin-bottom: 10px;
                    text-decoration: underline;
                }
                .invoice-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-bottom: 30px;
                }
                .invoice-table th,
                .invoice-table td {
                    border: 1px solid #000;
                    padding: 8px;
                    text-align: left;
                }
                .invoice-table th {
                    background-color: #f0f0f0;
                    font-weight: bold;
                    text-align: center;
                }
                .text-right {
                    text-align: right;
                }
                .text-center {
                    text-align: center;
                }
                .invoice-summary {
                    float: right;
                    width: 300px;
                    margin-top: 20px;
                }
                .summary-row {
                    display: flex;
                    justify-content: space-between;
                    padding: 5px 0;
                    border-bottom: 1px solid #ccc;
                }
                .summary-row.total {
                    font-weight: bold;
                    border-bottom: 2px solid #000;
                    font-size: 14pt;
                }
                .invoice-footer {
                    margin-top: 50px;
                    clear: both;
                }
                .signature-section {
                    display: flex;
                    justify-content: space-between;
                    margin-top: 50px;
                }
                .signature-box {
                    text-align: center;
                    width: 200px;
                }
                .signature-line {
                    border-top: 1px solid #000;
                    margin-top: 60px;
                    padding-top: 5px;
                }
            ");
            printableHtml.AppendLine("</style>");
            printableHtml.AppendLine("</head>");
            printableHtml.AppendLine("<body>");
            printableHtml.AppendLine(html);
            printableHtml.AppendLine("</body>");
            printableHtml.AppendLine("</html>");
            
            return printableHtml.ToString();
        }

        /// <summary>
        /// Get PDF content type for HTTP responses
        /// </summary>
        /// <returns>PDF content type</returns>
        public static string GetPdfContentType()
        {
            return "application/pdf";
        }

        /// <summary>
        /// Get content disposition header for PDF files
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="inline">Whether to display inline or as attachment</param>
        /// <returns>Content disposition header value</returns>
        public static string GetContentDispositionHeader(string fileName, bool inline = false)
        {
            var disposition = inline ? "inline" : "attachment";
            var encodedFileName = Uri.EscapeDataString(fileName);
            return $"{disposition}; filename=\"{fileName}\"; filename*=UTF-8''{encodedFileName}";
        }

        /// <summary>
        /// Format currency for Vietnamese locale
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("#,##0") + " VNĐ";
        }

        /// <summary>
        /// Format date for Vietnamese locale
        /// </summary>
        /// <param name="date">Date to format</param>
        /// <returns>Formatted date string</returns>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Format date with time for Vietnamese locale
        /// </summary>
        /// <param name="date">Date to format</param>
        /// <returns>Formatted date time string</returns>
        public static string FormatDateTime(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm");
        }
    }
}

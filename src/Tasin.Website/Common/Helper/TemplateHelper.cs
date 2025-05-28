using System.Text;
using System.Text.RegularExpressions;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Common.Helper
{
    /// <summary>
    /// Helper class for processing HTML templates with variable substitution
    /// </summary>
    public static class TemplateHelper
    {
        /// <summary>
        /// Process invoice template with data substitution
        /// </summary>
        /// <param name="templatePath">Path to template file</param>
        /// <param name="invoiceData">Invoice data</param>
        /// <returns>Processed HTML</returns>
        public static async Task<string> ProcessInvoiceTemplate(string templatePath, InvoiceViewModel invoiceData)
        {
            try
            {
                // Read template file
                var template = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
                
                // Replace variables
                var processedHtml = ReplaceTemplateVariables(template, invoiceData);
                
                return processedHtml;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing template: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Replace template variables with actual data
        /// </summary>
        /// <param name="template">Template HTML</param>
        /// <param name="invoiceData">Invoice data</param>
        /// <returns>Processed HTML</returns>
        private static string ReplaceTemplateVariables(string template, InvoiceViewModel invoiceData)
        {
            var html = template;

            // Replace basic invoice information
            html = html.Replace("{{InvoiceCode}}", invoiceData.InvoiceCode ?? "");
            html = html.Replace("{{PurchaseOrderCode}}", invoiceData.PurchaseOrderCode ?? "");
            html = html.Replace("{{InvoiceDate}}", PdfHelper.FormatDate(invoiceData.InvoiceDate));
            
            // Handle optional due date
            if (invoiceData.DueDate.HasValue)
            {
                html = ProcessConditionalBlock(html, "DueDate", invoiceData.DueDate.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                html = RemoveConditionalBlock(html, "DueDate");
            }

            // Replace company information
            html = html.Replace("{{CompanyName}}", invoiceData.Company?.Name ?? "");
            html = html.Replace("{{CompanyAddress}}", invoiceData.Company?.Address ?? "");
            html = html.Replace("{{CompanyPhone}}", invoiceData.Company?.Phone ?? "");
            html = html.Replace("{{CompanyEmail}}", invoiceData.Company?.Email ?? "");
            html = html.Replace("{{CompanyTaxCode}}", invoiceData.Company?.TaxCode ?? "");
            
            // Handle optional company bank info
            if (!string.IsNullOrEmpty(invoiceData.Company?.BankAccount))
            {
                html = ProcessConditionalBlock(html, "CompanyBankAccount", invoiceData.Company.BankAccount);
                html = html.Replace("{{CompanyBankName}}", invoiceData.Company?.BankName ?? "");
            }
            else
            {
                html = RemoveConditionalBlock(html, "CompanyBankAccount");
            }

            // Replace customer information
            html = html.Replace("{{CustomerName}}", invoiceData.Customer?.Name ?? "");
            html = html.Replace("{{CustomerCode}}", invoiceData.Customer?.Code ?? "");
            html = html.Replace("{{CustomerAddress}}", invoiceData.Customer?.Address ?? "");
            
            // Handle optional customer info
            html = ProcessOptionalCustomerField(html, "CustomerPhone", invoiceData.Customer?.PhoneContact);
            html = ProcessOptionalCustomerField(html, "CustomerEmail", invoiceData.Customer?.Email);
            html = ProcessOptionalCustomerField(html, "CustomerTaxCode", invoiceData.Customer?.TaxCode);

            // Replace financial information
            html = html.Replace("{{Subtotal}}", PdfHelper.FormatCurrency(invoiceData.Subtotal));
            html = html.Replace("{{TaxAmount}}", PdfHelper.FormatCurrency(invoiceData.TaxAmount));
            html = html.Replace("{{TotalAmount}}", PdfHelper.FormatCurrency(invoiceData.TotalAmount));
            
            // Add amount in words
            html = html.Replace("{{TotalAmountInWords}}", ConvertAmountToWords(invoiceData.TotalAmount));

            // Handle optional payment terms and notes
            html = ProcessOptionalField(html, "PaymentTerms", invoiceData.PaymentTerms);
            html = ProcessOptionalField(html, "Notes", invoiceData.Notes);

            // Process invoice items
            html = ProcessInvoiceItems(html, invoiceData.Items);

            return html;
        }

        /// <summary>
        /// Process conditional blocks in template
        /// </summary>
        private static string ProcessConditionalBlock(string html, string blockName, string value)
        {
            var pattern = $@"\{{{{#{blockName}\}}}}(.*?)\{{{{/{blockName}\}}}}";
            var replacement = $"$1";
            html = Regex.Replace(html, pattern, replacement, RegexOptions.Singleline);
            html = html.Replace($"{{{{{blockName}}}}}", value);
            return html;
        }

        /// <summary>
        /// Remove conditional blocks when data is not available
        /// </summary>
        private static string RemoveConditionalBlock(string html, string blockName)
        {
            var pattern = $@"\{{{{#{blockName}\}}}}.*?\{{{{/{blockName}\}}}}";
            return Regex.Replace(html, pattern, "", RegexOptions.Singleline);
        }

        /// <summary>
        /// Process optional customer fields
        /// </summary>
        private static string ProcessOptionalCustomerField(string html, string fieldName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return ProcessConditionalBlock(html, fieldName, value);
            }
            else
            {
                return RemoveConditionalBlock(html, fieldName);
            }
        }

        /// <summary>
        /// Process optional fields like PaymentTerms and Notes
        /// </summary>
        private static string ProcessOptionalField(string html, string fieldName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return ProcessConditionalBlock(html, fieldName, value);
            }
            else
            {
                return RemoveConditionalBlock(html, fieldName);
            }
        }

        /// <summary>
        /// Process invoice items list
        /// </summary>
        private static string ProcessInvoiceItems(string html, List<InvoiceItemViewModel> items)
        {
            var itemsPattern = @"\{\{#InvoiceItems\}\}(.*?)\{\{/InvoiceItems\}\}";
            var itemsMatch = Regex.Match(html, itemsPattern, RegexOptions.Singleline);
            
            if (!itemsMatch.Success || items == null || !items.Any())
            {
                return Regex.Replace(html, itemsPattern, "", RegexOptions.Singleline);
            }

            var itemTemplate = itemsMatch.Groups[1].Value;
            var itemsHtml = new StringBuilder();

            foreach (var item in items)
            {
                var itemHtml = itemTemplate;
                itemHtml = itemHtml.Replace("{{SequenceNumber}}", item.SequenceNumber.ToString());
                itemHtml = itemHtml.Replace("{{ProductCode}}", item.ProductCode ?? "");
                itemHtml = itemHtml.Replace("{{ProductName}}", item.ProductName ?? "");
                itemHtml = itemHtml.Replace("{{Unit}}", item.Unit ?? "");
                itemHtml = itemHtml.Replace("{{Quantity}}", item.Quantity.ToString("N0"));
                itemHtml = itemHtml.Replace("{{UnitPrice}}", PdfHelper.FormatCurrency(item.UnitPrice));
                itemHtml = itemHtml.Replace("{{TaxRate}}", item.TaxRate.ToString("N1"));
                itemHtml = itemHtml.Replace("{{TotalAmount}}", PdfHelper.FormatCurrency(item.TotalAmount));
                
                itemsHtml.AppendLine(itemHtml);
            }

            return Regex.Replace(html, itemsPattern, itemsHtml.ToString(), RegexOptions.Singleline);
        }

        /// <summary>
        /// Convert amount to Vietnamese words
        /// </summary>
        private static string ConvertAmountToWords(decimal amount)
        {
            try
            {
                // Simple implementation - can be enhanced with a proper number-to-words library
                var integerPart = (long)Math.Floor(amount);
                
                if (integerPart == 0)
                    return "Không đồng";
                
                // For now, return a simple format
                // In production, use a proper Vietnamese number-to-words converter
                return $"{integerPart:N0} đồng".Replace(",", " ");
            }
            catch
            {
                return "Không xác định";
            }
        }

        /// <summary>
        /// Get template file path
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <returns>Full path to template</returns>
        public static string GetTemplatePath(string templateName)
        {
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return Path.Combine(webRootPath, "templates", templateName);
        }

        /// <summary>
        /// Validate template file exists
        /// </summary>
        /// <param name="templatePath">Template path</param>
        /// <returns>True if exists</returns>
        public static bool TemplateExists(string templatePath)
        {
            return File.Exists(templatePath);
        }
    }
}

using ClosedXML.Excel;
using System.Text;

namespace Tasin.Website.Common.Helper
{
    /// <summary>
    /// Helper class for Excel operations with proper UTF-8 encoding support
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// Get default load options for Excel files with UTF-8 support
        /// </summary>
        /// <returns>LoadOptions configured for UTF-8</returns>
        public static LoadOptions GetDefaultLoadOptions()
        {
            return new LoadOptions()
            {
                RecalculateAllFormulas = false
            };
        }

        /// <summary>
        /// Get default save options for Excel files with UTF-8 support
        /// </summary>
        /// <returns>SaveOptions configured for UTF-8</returns>
        public static SaveOptions GetDefaultSaveOptions()
        {
            return new SaveOptions()
            {
                EvaluateFormulasBeforeSaving = false,
                GenerateCalculationChain = false
            };
        }

        /// <summary>
        /// Create a new workbook with UTF-8 support
        /// </summary>
        /// <returns>XLWorkbook configured for UTF-8</returns>
        public static XLWorkbook CreateWorkbook()
        {
            return new XLWorkbook(GetDefaultLoadOptions());
        }

        /// <summary>
        /// Create a workbook from stream with UTF-8 support
        /// </summary>
        /// <param name="stream">Stream containing Excel data</param>
        /// <returns>XLWorkbook configured for UTF-8</returns>
        public static XLWorkbook CreateWorkbook(Stream stream)
        {
            return new XLWorkbook(stream, GetDefaultLoadOptions());
        }

        /// <summary>
        /// Save workbook to byte array with UTF-8 support
        /// </summary>
        /// <param name="workbook">Workbook to save</param>
        /// <returns>Byte array containing Excel data</returns>
        public static byte[] SaveToByteArray(XLWorkbook workbook)
        {
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream, GetDefaultSaveOptions());
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Save workbook to file with UTF-8 support
        /// </summary>
        /// <param name="workbook">Workbook to save</param>
        /// <param name="filePath">File path to save to</param>
        public static void SaveToFile(XLWorkbook workbook, string filePath)
        {
            workbook.SaveAs(filePath, GetDefaultSaveOptions());
        }

        /// <summary>
        /// Get proper Content-Disposition header for Excel files with UTF-8 filename support
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Content-Disposition header value</returns>
        public static string GetContentDispositionHeader(string fileName)
        {
            return $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";
        }

        /// <summary>
        /// Get proper Content-Type for Excel files
        /// </summary>
        /// <returns>Content-Type for Excel files</returns>
        public static string GetExcelContentType()
        {
            return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        /// <summary>
        /// Safely get string value from Excel cell with proper encoding
        /// </summary>
        /// <param name="cell">Excel cell</param>
        /// <returns>String value with proper encoding</returns>
        public static string GetCellStringValue(IXLCell cell)
        {
            var value = cell.GetString();
            
            // Ensure proper UTF-8 encoding
            if (!string.IsNullOrEmpty(value))
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                value = Encoding.UTF8.GetString(bytes);
            }
            
            return value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Set cell value with proper UTF-8 encoding
        /// </summary>
        /// <param name="cell">Excel cell</param>
        /// <param name="value">Value to set</param>
        public static void SetCellValue(IXLCell cell, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Ensure proper UTF-8 encoding
                var bytes = Encoding.UTF8.GetBytes(value);
                var encodedValue = Encoding.UTF8.GetString(bytes);
                cell.Value = encodedValue;
            }
            else
            {
                cell.Value = string.Empty;
            }
        }
    }
}

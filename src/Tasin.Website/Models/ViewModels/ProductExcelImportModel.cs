using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// Model for importing products from Excel
    /// </summary>
    public class ProductExcelImportModel
    {
        /// <summary>
        /// Row number in Excel (for error reporting)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Product name (required)
        /// </summary>
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// English name (optional)
        /// </summary>
        public string? Name_EN { get; set; }

        /// <summary>
        /// Unit code (will be mapped to Unit_ID)
        /// </summary>
        public string? UnitCode { get; set; }

        /// <summary>
        /// Category code (will be mapped to Category_ID)
        /// </summary>
        public string? CategoryCode { get; set; }

        /// <summary>
        /// Processing type text (Nguyên liệu/Sơ chế/Thành phẩm)
        /// </summary>
        public string? ProcessingTypeText { get; set; }

        /// <summary>
        /// Special product tax rate code (will be mapped to SpecialProductTaxRate_ID)
        /// </summary>
        public string? SpecialProductTaxRateCode { get; set; }

        /// <summary>
        /// Tax rate (%)
        /// </summary>
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Loss rate (%)
        /// </summary>
        public decimal? LossRate { get; set; }

        /// <summary>
        /// Profit margin (%)
        /// </summary>
        public decimal? ProfitMargin { get; set; }

        /// <summary>
        /// Processing fee
        /// </summary>
        public decimal? ProcessingFee { get; set; }

        /// <summary>
        /// Default price
        /// </summary>
        public decimal? DefaultPrice { get; set; }

        /// <summary>
        /// Company tax rate (%)
        /// </summary>
        public decimal? CompanyTaxRate { get; set; }

        /// <summary>
        /// Consumer tax rate (%)
        /// </summary>
        public decimal? ConsumerTaxRate { get; set; }

        /// <summary>
        /// Note
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Is discontinued (Y/N)
        /// </summary>
        public string? IsDiscontinuedText { get; set; }

        /// <summary>
        /// Parsed IsDiscontinued value
        /// </summary>
        public bool IsDiscontinued =>
            !string.IsNullOrEmpty(IsDiscontinuedText) &&
            (IsDiscontinuedText.Trim().ToUpper() == "Y" ||
             IsDiscontinuedText.Trim().ToUpper() == "YES" ||
             IsDiscontinuedText.Trim().ToUpper() == "TRUE" ||
             IsDiscontinuedText.Trim() == "1");

        /// <summary>
        /// Parsed ProcessingType value as enum
        /// </summary>
        public EProcessingType ProcessingType
        {
            get
            {
                if (string.IsNullOrEmpty(ProcessingTypeText))
                    return EProcessingType.Material;

                var text = ProcessingTypeText.Trim();

                // First try to parse as enum name (case-insensitive)
                if (Enum.TryParse<EProcessingType>(text, true, out var enumResult))
                {
                    return enumResult;
                }

                // Fallback to description matching for backward compatibility
                var textLower = text.ToLower();
                return textLower switch
                {
                    "nguyên liệu" or "nguyen lieu" or "material" => EProcessingType.Material,
                    "sơ chế" or "so che" or "semiprocessed" or "semi-processed" => EProcessingType.SemiProcessed,
                    "thành phẩm" or "thanh pham" or "finished" or "finishedproduct" or "finished-product" => EProcessingType.FinishedProduct,
                    _ => EProcessingType.Material
                };
            }
        }

        /// <summary>
        /// Validation errors for this row
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result model for Excel import operation
    /// </summary>
    public class ProductExcelImportResult
    {
        /// <summary>
        /// Total rows processed
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Successfully imported rows
        /// </summary>
        public int SuccessfulRows { get; set; }

        /// <summary>
        /// Failed rows
        /// </summary>
        public int FailedRows { get; set; }

        /// <summary>
        /// List of errors by row
        /// </summary>
        public List<ProductExcelImportError> Errors { get; set; } = new List<ProductExcelImportError>();

        /// <summary>
        /// Overall success status
        /// </summary>
        public bool IsSuccess => FailedRows == 0;

        /// <summary>
        /// Summary message
        /// </summary>
        public string Message => $"Đã xử lý {TotalRows} dòng. Thành công: {SuccessfulRows}, Thất bại: {FailedRows}";
    }

    /// <summary>
    /// Error information for a specific row
    /// </summary>
    public class ProductExcelImportError
    {
        /// <summary>
        /// Row number
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Product name from the row
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// List of error messages
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}

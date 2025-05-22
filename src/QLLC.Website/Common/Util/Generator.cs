using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading.Tasks;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Common.Util
{
    public static class Generator
    {
        /// <summary>
        /// Generates a code based on the CodeVersion table
        /// </summary>
        /// <param name="type">The type of code to generate</param>
        /// <param name="context">Database context</param>
        /// <returns>Generated code</returns>
        public static async Task<string> GenerateCodeAsync(string type, SampleDBContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database context cannot be null");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty", nameof(type));

            // Find the code version record for the specified type
            var codeVersion = await context.CodeVersions
                .FirstOrDefaultAsync(cv => cv.Type == type);

            if (codeVersion == null)
                throw new InvalidOperationException($"No code version found for type: {type}");

            // Increment the version index
            codeVersion.VersionIndex++;

            // Format the code with the prefix and the incremented version index
            string generatedCode = $"{codeVersion.Prefix}_{codeVersion.VersionIndex:D6}";

            // Update the code version in the database
            await context.SaveChangesAsync();

            return generatedCode;
        }

        /// <summary>
        /// Generates a code based on the CodeVersion table with a custom format
        /// </summary>
        /// <param name="type">The type of code to generate</param>
        /// <param name="context">Database context</param>
        /// <param name="format">Custom format string where {0} is the prefix and {1} is the version index</param>
        /// <returns>Generated code</returns>
        public static async Task<string> GenerateCodeAsync(string type, SampleDBContext context, string format)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database context cannot be null");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty", nameof(type));

            if (string.IsNullOrEmpty(format))
                throw new ArgumentException("Format cannot be null or empty", nameof(format));

            // Find the code version record for the specified type
            var codeVersion = await context.CodeVersions
                .FirstOrDefaultAsync(cv => cv.Type == type);

            if (codeVersion == null)
                throw new InvalidOperationException($"No code version found for type: {type}");

            // Increment the version index
            codeVersion.VersionIndex++;

            // Format the code with the custom format
            string generatedCode = string.Format(format, codeVersion.Prefix, codeVersion.VersionIndex);

            // Update the code version in the database
            await context.SaveChangesAsync();

            return generatedCode;
        }

        /// <summary>
        /// Generates a code based on the CodeVersion table with advanced options
        /// </summary>
        /// <param name="type">The type of code to generate</param>
        /// <param name="context">Database context</param>
        /// <param name="options">Options for code generation</param>
        /// <returns>Generated code</returns>
        public static async Task<string> GenerateCodeAsync(string type, SampleDBContext context, CodeGenerationOptions options)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database context cannot be null");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty", nameof(type));

            if (options == null)
                throw new ArgumentNullException(nameof(options), "Options cannot be null");

            // Find the code version record for the specified type
            var codeVersion = await context.CodeVersions
                .FirstOrDefaultAsync(cv => cv.Type == type);

            if (codeVersion == null)
                throw new InvalidOperationException($"No code version found for type: {type}");

            // Increment the version index
            codeVersion.VersionIndex++;

            // Build the code based on options
            string generatedCode;

            if (!string.IsNullOrEmpty(options.CustomFormat))
            {
                // Use custom format if provided
                generatedCode = string.Format(options.CustomFormat, codeVersion.Prefix, codeVersion.VersionIndex);
            }
            else
            {
                // Build the code with the configured options
                string versionPart = codeVersion.VersionIndex.ToString(options.DigitFormat ?? "D6");

                // Add date part if requested
                string datePart = string.Empty;
                if (options.IncludeDate)
                {
                    DateTime dateToUse = options.UseUtcDate ? DateTime.UtcNow : DateTime.Now;
                    datePart = dateToUse.ToString(options.DateFormat ?? "yyyyMMdd");
                }

                // Add separator if both prefix and version are used
                string separator = options.UseSeparator ? options.Separator ?? "-" : string.Empty;

                // Build the final code
                generatedCode = $"{codeVersion.Prefix}{separator}{datePart}{(string.IsNullOrEmpty(datePart) ? "" : separator)}{versionPart}";
            }

            // Add suffix if provided
            if (!string.IsNullOrEmpty(options.Suffix))
            {
                generatedCode += options.Suffix;
            }

            // Update the code version in the database
            await context.SaveChangesAsync();

            return generatedCode;
        }

        /// <summary>
        /// Options for code generation
        /// </summary>
        public class CodeGenerationOptions
        {
            /// <summary>
            /// Custom format string where {0} is the prefix and {1} is the version index
            /// </summary>
            public string? CustomFormat { get; set; }

            /// <summary>
            /// Format string for the version digits (e.g., "D6" for 6 digits with leading zeros)
            /// </summary>
            public string? DigitFormat { get; set; } = "D6";

            /// <summary>
            /// Whether to include the current date in the code
            /// </summary>
            public bool IncludeDate { get; set; }

            /// <summary>
            /// Format string for the date (e.g., "yyyyMMdd")
            /// </summary>
            public string? DateFormat { get; set; } = "yyyyMMdd";

            /// <summary>
            /// Whether to use UTC date instead of local date
            /// </summary>
            public bool UseUtcDate { get; set; }

            /// <summary>
            /// Whether to use a separator between parts of the code
            /// </summary>
            public bool UseSeparator { get; set; }

            /// <summary>
            /// Separator character or string to use between parts of the code
            /// </summary>
            public string? Separator { get; set; } = "-";

            /// <summary>
            /// Optional suffix to append to the end of the code
            /// </summary>
            public string? Suffix { get; set; }
        }

        /// <summary>
        /// Creates a new CodeVersion entry if one doesn't exist for the specified type
        /// </summary>
        /// <param name="type">The type of code</param>
        /// <param name="prefix">The prefix to use for this code type</param>
        /// <param name="context">Database context</param>
        /// <param name="initialIndex">Initial version index (default is 0)</param>
        /// <param name="note">Optional note for this code type</param>
        /// <returns>The created or existing CodeVersion entity</returns>
        public static async Task<CodeVersion> EnsureCodeVersionExistsAsync(
            string type,
            string prefix,
            SampleDBContext context,
            int initialIndex = 0,
            string? note = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database context cannot be null");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty", nameof(type));

            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));

            // Check if the code version already exists
            var codeVersion = await context.CodeVersions
                .FirstOrDefaultAsync(cv => cv.Type == type);

            if (codeVersion == null)
            {
                // Create a new code version entry
                codeVersion = new CodeVersion
                {
                    Type = type,
                    Prefix = prefix,
                    VersionIndex = initialIndex,
                    Note = note
                };

                await context.CodeVersions.AddAsync(codeVersion);
                await context.SaveChangesAsync();
            }

            return codeVersion;
        }

        /// <summary>
        /// Generates a code for a common entity type
        /// </summary>
        /// <param name="entityType">The entity type (e.g., "Customer", "Vendor", "Product")</param>
        /// <param name="context">Database context</param>
        /// <returns>Generated code</returns>
        public static async Task<string> GenerateEntityCodeAsync(string entityType, SampleDBContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database context cannot be null");

            if (string.IsNullOrEmpty(entityType))
                throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));

            // Define default prefixes for common entity types
            string prefix = entityType.ToUpper() switch
            {
                "CUSTOMER" => "CUS",
                "VENDOR" => "VEN",
                "PRODUCT" => "PRD",
                "CATEGORY" => "CAT",
                "UNIT" => "UNT",
                "MATERIAL" => "MAT",
                "PURCHASE_ORDER" => "PO",
                "PURCHASE_AGREEMENT" => "PA",
                // For unknown entity types, use the first three letters
                _ => entityType.Length >= 3
                    ? entityType[..3].ToUpper()
                    : entityType.ToUpper().PadRight(3, 'X')
            };

            // Ensure the code version exists
            await EnsureCodeVersionExistsAsync(entityType, prefix, context);

            // Generate the code with default options
            return await GenerateCodeAsync(entityType, context);
        }

        /// <summary>
        /// Generates a code with a year-month pattern (commonly used for invoices, orders, etc.)
        /// </summary>
        /// <param name="type">The type of code to generate</param>
        /// <param name="context">Database context</param>
        /// <param name="useCurrentDate">Whether to use the current date or a specific date</param>
        /// <param name="specificDate">Specific date to use if useCurrentDate is false</param>
        /// <param name="yearFormat">Format for the year part (default is "yy" for 2-digit year)</param>
        /// <param name="monthFormat">Format for the month part (default is "MM" for 2-digit month)</param>
        /// <param name="digitFormat">Format for the sequence number (default is "D4" for 4 digits with leading zeros)</param>
        /// <returns>Generated code with year-month pattern</returns>
        public static async Task<string> GenerateYearMonthCodeAsync(
            string type,
            SampleDBContext context,
            bool useCurrentDate = true,
            DateTime? specificDate = null,
            string yearFormat = "yy",
            string monthFormat = "MM",
            string digitFormat = "D4")
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database context cannot be null");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty", nameof(type));

            // Determine the date to use
            DateTime dateToUse = useCurrentDate ? DateTime.Now : specificDate ?? DateTime.Now;

            // Create a type that includes the year and month
            string yearMonthType = $"{type}_{dateToUse.ToString(yearFormat)}_{dateToUse.ToString(monthFormat)}";

            // Find or create the code version for this year-month combination
            var codeVersion = await context.CodeVersions
                .FirstOrDefaultAsync(cv => cv.Type == yearMonthType);

            if (codeVersion == null)
            {
                // Get the base type to use its prefix
                var baseCodeVersion = await context.CodeVersions
                    .FirstOrDefaultAsync(cv => cv.Type == type);

                string prefix = baseCodeVersion?.Prefix ?? type[..Math.Min(3, type.Length)].ToUpper();

                // Create a new code version entry for this year-month
                codeVersion = new CodeVersion
                {
                    Type = yearMonthType,
                    Prefix = prefix,
                    VersionIndex = 0,
                    Note = $"Auto-generated for {type} - {dateToUse.ToString("yyyy-MM")}"
                };

                await context.CodeVersions.AddAsync(codeVersion);
            }

            // Increment the version index
            codeVersion.VersionIndex++;

            // Format the code: PREFIX-YYMM-NNNN
            string yearPart = dateToUse.ToString(yearFormat);
            string monthPart = dateToUse.ToString(monthFormat);
            string sequencePart = codeVersion.VersionIndex.ToString(digitFormat);

            string generatedCode = $"{codeVersion.Prefix}-{yearPart}{monthPart}-{sequencePart}";

            // Save changes to the database
            await context.SaveChangesAsync();

            return generatedCode;
        }

        /// <summary>
        /// Generates a random code with specified options
        /// </summary>
        /// <param name="prefix">Optional prefix for the code</param>
        /// <param name="length">Length of the random part (default is 8)</param>
        /// <param name="useLetters">Whether to include letters in the random part (default is true)</param>
        /// <param name="useNumbers">Whether to include numbers in the random part (default is true)</param>
        /// <param name="useSpecialChars">Whether to include special characters in the random part (default is false)</param>
        /// <param name="separator">Optional separator between prefix and random part</param>
        /// <returns>Generated random code</returns>
        public static string GenerateRandomCode(
            string? prefix = null,
            int length = 8,
            bool useLetters = true,
            bool useNumbers = true,
            bool useSpecialChars = false,
            string? separator = null)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than zero", nameof(length));

            if (!useLetters && !useNumbers && !useSpecialChars)
                throw new ArgumentException("At least one character type (letters, numbers, or special characters) must be used");

            // Define character sets
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Excluding I and O which can be confused with 1 and 0
            const string numbers = "23456789"; // Excluding 0 and 1 which can be confused with O and I
            const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            // Build the character set to use
            var charSet = new StringBuilder();
            if (useLetters) charSet.Append(letters);
            if (useNumbers) charSet.Append(numbers);
            if (useSpecialChars) charSet.Append(specialChars);

            // Generate the random part
            var random = new Random();
            var randomPart = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, charSet.Length);
                randomPart.Append(charSet[index]);
            }

            // Build the final code
            var code = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix))
            {
                code.Append(prefix);
                if (!string.IsNullOrEmpty(separator))
                    code.Append(separator);
            }
            code.Append(randomPart);

            return code.ToString();
        }
    }
}

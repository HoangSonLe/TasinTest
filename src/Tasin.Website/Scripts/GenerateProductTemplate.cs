using ClosedXML.Excel;
using Tasin.Website.Common.Helper;

namespace Tasin.Website.Scripts
{
    /// <summary>
    /// Script to generate static Product Excel template
    /// </summary>
    public static class GenerateProductTemplate
    {
        public static void CreateTemplate()
        {
            using (var workbook = ExcelHelper.CreateWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");

                // Set headers
                var headers = new[]
                {
                    "Tên sản phẩm (*)",
                    "Tên tiếng Anh",
                    "Mã đơn vị",
                    "Mã danh mục",
                    "Mã loại chế biến",
                    "Là nguyên liệu (Y/N)",
                    "Mã thuế suất đặc biệt",
                    "Thuế suất (%)",
                    "Tỷ lệ hao hụt (%)",
                    "Tỷ lệ lợi nhuận (%)",
                    "Phí chế biến",
                    "Đơn giá mặc định",
                    "Thuế suất công ty (%)",
                    "Thuế suất người tiêu dùng (%)",
                    "Ghi chú",
                    "Ngừng sản xuất (Y/N)"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    ExcelHelper.SetCellValue(cell, headers[i]);
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // Add sample data
                ExcelHelper.SetCellValue(worksheet.Cell(2, 1), "Sản phẩm mẫu 1");
                ExcelHelper.SetCellValue(worksheet.Cell(2, 2), "Sample Product 1");
                worksheet.Cell(2, 3).Value = "KG";
                worksheet.Cell(2, 4).Value = "CAT001";
                worksheet.Cell(2, 5).Value = "PT001";
                worksheet.Cell(2, 6).Value = "Y";
                worksheet.Cell(2, 7).Value = "SPTR001";
                worksheet.Cell(2, 8).Value = 10;
                worksheet.Cell(2, 9).Value = 5;
                worksheet.Cell(2, 10).Value = 15;
                worksheet.Cell(2, 11).Value = 1000;
                worksheet.Cell(2, 12).Value = 50000;
                worksheet.Cell(2, 13).Value = 8;
                worksheet.Cell(2, 14).Value = 10;
                worksheet.Cell(2, 15).Value = "Ghi chú mẫu";
                worksheet.Cell(2, 16).Value = "N";

                // Add more sample data
                worksheet.Cell(3, 1).Value = "Sản phẩm mẫu 2";
                worksheet.Cell(3, 2).Value = "Sample Product 2";
                worksheet.Cell(3, 3).Value = "THUNG";
                worksheet.Cell(3, 4).Value = "CAT002";
                worksheet.Cell(3, 5).Value = "PT002";
                worksheet.Cell(3, 6).Value = "N";
                worksheet.Cell(3, 7).Value = "";
                worksheet.Cell(3, 8).Value = 8;
                worksheet.Cell(3, 9).Value = 3;
                worksheet.Cell(3, 10).Value = 20;
                worksheet.Cell(3, 11).Value = 2000;
                worksheet.Cell(3, 12).Value = 75000;
                worksheet.Cell(3, 13).Value = 10;
                worksheet.Cell(3, 14).Value = 12;
                worksheet.Cell(3, 15).Value = "";
                worksheet.Cell(3, 16).Value = "Y";

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Add instructions sheet
                var instructionSheet = workbook.Worksheets.Add("Hướng dẫn");
                instructionSheet.Cell(1, 1).Value = "HƯỚNG DẪN IMPORT SẢN PHẨM";
                instructionSheet.Cell(1, 1).Style.Font.Bold = true;
                instructionSheet.Cell(1, 1).Style.Font.FontSize = 16;

                var instructions = new[]
                {
                    "",
                    "1. Các cột bắt buộc:",
                    "   - Tên sản phẩm (*): Không được để trống",
                    "   - Thuế suất công ty (%): Bắt buộc nhập số",
                    "   - Thuế suất người tiêu dùng (%): Bắt buộc nhập số",
                    "",
                    "2. Các cột tùy chọn:",
                    "   - Tên tiếng Anh: Có thể để trống",
                    "   - Mã đơn vị: Phải tồn tại trong hệ thống (ví dụ: KG, THUNG, CAI)",
                    "   - Mã danh mục: Phải tồn tại trong hệ thống",
                    "   - Mã loại chế biến: Phải tồn tại trong hệ thống",
                    "   - Là nguyên liệu: Nhập Y/N, Yes/No, True/False, 1/0",
                    "   - Mã thuế suất đặc biệt: Phải tồn tại trong hệ thống",
                    "   - Các tỷ lệ %: Nhập số thập phân (ví dụ: 10.5)",
                    "   - Phí chế biến: Nhập số",
                    "   - Đơn giá mặc định: Nhập số (ví dụ: 50000)",
                    "   - Ghi chú: Có thể để trống",
                    "   - Ngừng sản xuất: Nhập Y/N, Yes/No, True/False, 1/0",
                    "",
                    "3. Lưu ý:",
                    "   - Không được xóa dòng tiêu đề",
                    "   - Dữ liệu bắt đầu từ dòng 2",
                    "   - File phải có định dạng Excel (.xlsx, .xls, .xlsm) hoặc CSV",
                    "   - Mã các thực thể liên quan phải tồn tại trong hệ thống",
                    "   - Xóa dữ liệu mẫu trước khi nhập dữ liệu thực tế",
                    "",
                    "4. Các mã thường dùng:",
                    "   - Đơn vị: KG, THUNG, CAI, GOI, HOP",
                    "   - Danh mục: CAT001, CAT002, CAT003",
                    "   - Loại chế biến: PT001, PT002, PT003",
                    "   - Nguyên liệu: MAT001, MAT002, MAT003"
                };

                for (int i = 0; i < instructions.Length; i++)
                {
                    ExcelHelper.SetCellValue(instructionSheet.Cell(i + 2, 1), instructions[i]);
                }

                instructionSheet.Columns().AdjustToContents();

                // Save the template
                var templatePath = Path.Combine("wwwroot", "templates", "Product_Import_Template.xlsx");
                ExcelHelper.SaveToFile(workbook, templatePath);
            }
        }
    }
}

using ClosedXML.Excel;
using Tasin.Website.Common.Helper;

namespace Tasin.Website.Scripts
{
    /// <summary>
    /// Test script to verify Excel UTF-8 functionality
    /// </summary>
    public static class TestExcelUTF8
    {
        /// <summary>
        /// Test creating Excel file with Vietnamese characters
        /// </summary>
        public static void TestCreateExcelWithVietnamese()
        {
            try
            {
                Console.WriteLine("Testing Excel UTF-8 functionality...");

                using (var workbook = ExcelHelper.CreateWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Test Sheet");

                    // Test Vietnamese characters
                    var testData = new[]
                    {
                        "Tên sản phẩm",
                        "Mô tả chi tiết",
                        "Giá bán (VNĐ)",
                        "Số lượng tồn kho",
                        "Danh mục sản phẩm",
                        "Nhà cung cấp",
                        "Ghi chú đặc biệt",
                        "Trạng thái hoạt động",
                        "Ngày tạo",
                        "Người cập nhật"
                    };

                    // Set headers
                    for (int i = 0; i < testData.Length; i++)
                    {
                        var cell = worksheet.Cell(1, i + 1);
                        ExcelHelper.SetCellValue(cell, testData[i]);
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    }

                    // Add sample data with Vietnamese characters
                    var sampleData = new[]
                    {
                        "Bánh mì Việt Nam",
                        "Bánh mì truyền thống với thịt nướng và rau sống",
                        "25,000",
                        "100",
                        "Thực phẩm nhanh",
                        "Công ty TNHH ABC",
                        "Sản phẩm đặc sản miền Nam",
                        "Đang hoạt động",
                        "01/01/2024",
                        "Nguyễn Văn A"
                    };

                    for (int i = 0; i < sampleData.Length; i++)
                    {
                        ExcelHelper.SetCellValue(worksheet.Cell(2, i + 1), sampleData[i]);
                    }

                    // Add more test data
                    var moreTestData = new[]
                    {
                        "Phở bò Hà Nội",
                        "Món ăn truyền thống của người Việt",
                        "45,000",
                        "50",
                        "Món ăn chính",
                        "Nhà hàng XYZ",
                        "Đặc sản miền Bắc",
                        "Đang hoạt động",
                        "02/01/2024",
                        "Trần Thị B"
                    };

                    for (int i = 0; i < moreTestData.Length; i++)
                    {
                        ExcelHelper.SetCellValue(worksheet.Cell(3, i + 1), moreTestData[i]);
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Save to file
                    var testPath = Path.Combine("wwwroot", "templates", "Test_UTF8_Excel.xlsx");
                    
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(testPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    ExcelHelper.SaveToFile(workbook, testPath);

                    Console.WriteLine($"Test Excel file created successfully at: {testPath}");
                    Console.WriteLine("Please open the file and verify that Vietnamese characters display correctly.");

                    // Test reading the file back
                    TestReadExcelWithVietnamese(testPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Excel UTF-8: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test reading Excel file with Vietnamese characters
        /// </summary>
        /// <param name="filePath">Path to Excel file</param>
        public static void TestReadExcelWithVietnamese(string filePath)
        {
            try
            {
                Console.WriteLine("\nTesting reading Excel file...");

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var workbook = ExcelHelper.CreateWorkbook(fileStream))
                {
                    var worksheet = workbook.Worksheet(1);
                    
                    Console.WriteLine("Reading data from Excel:");
                    
                    // Read first 3 rows
                    for (int row = 1; row <= 3; row++)
                    {
                        Console.WriteLine($"Row {row}:");
                        for (int col = 1; col <= 10; col++)
                        {
                            var cellValue = ExcelHelper.GetCellStringValue(worksheet.Cell(row, col));
                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                Console.WriteLine($"  Column {col}: {cellValue}");
                            }
                        }
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("Excel reading test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Excel file: {ex.Message}");
            }
        }

        /// <summary>
        /// Test byte array operations
        /// </summary>
        public static void TestByteArrayOperations()
        {
            try
            {
                Console.WriteLine("\nTesting byte array operations...");

                using (var workbook = ExcelHelper.CreateWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Byte Test");
                    
                    ExcelHelper.SetCellValue(worksheet.Cell(1, 1), "Test tiếng Việt với dấu");
                    ExcelHelper.SetCellValue(worksheet.Cell(1, 2), "Ăn, ở, học, làm việc");
                    ExcelHelper.SetCellValue(worksheet.Cell(1, 3), "Đây là test UTF-8");

                    var bytes = ExcelHelper.SaveToByteArray(workbook);
                    
                    Console.WriteLine($"Excel file converted to byte array: {bytes.Length} bytes");

                    // Test reading from byte array
                    using (var stream = new MemoryStream(bytes))
                    using (var testWorkbook = ExcelHelper.CreateWorkbook(stream))
                    {
                        var testWorksheet = testWorkbook.Worksheet(1);
                        
                        var value1 = ExcelHelper.GetCellStringValue(testWorksheet.Cell(1, 1));
                        var value2 = ExcelHelper.GetCellStringValue(testWorksheet.Cell(1, 2));
                        var value3 = ExcelHelper.GetCellStringValue(testWorksheet.Cell(1, 3));
                        
                        Console.WriteLine($"Read back from byte array:");
                        Console.WriteLine($"  Value 1: {value1}");
                        Console.WriteLine($"  Value 2: {value2}");
                        Console.WriteLine($"  Value 3: {value3}");
                    }
                }

                Console.WriteLine("Byte array test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing byte array operations: {ex.Message}");
            }
        }

        /// <summary>
        /// Run all tests
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("=== Excel UTF-8 Test Suite ===");
            Console.WriteLine();

            TestCreateExcelWithVietnamese();
            TestByteArrayOperations();

            Console.WriteLine();
            Console.WriteLine("=== All tests completed ===");
        }
    }
}

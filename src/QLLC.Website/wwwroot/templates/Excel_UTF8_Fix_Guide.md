# Hướng dẫn khắc phục lỗi UTF-8 trong file Excel

## Vấn đề
File Excel được tạo từ hệ thống bị lỗi hiển thị ký tự tiếng Việt khi mở bằng Microsoft Excel hoặc các ứng dụng khác.

## Nguyên nhân
- Thiếu cấu hình encoding UTF-8 phù hợp khi tạo và đọc file Excel
- Không có xử lý đặc biệt cho ký tự tiếng Việt có dấu
- Content-Type và Content-Disposition header không được thiết lập đúng cách

## Giải pháp đã thực hiện

### 1. Tạo ExcelHelper Class
Đã tạo class `ExcelHelper` trong `src/QLLC.Website/Common/Helper/ExcelHelper.cs` để:
- Cung cấp các phương thức tiện ích cho việc xử lý Excel với UTF-8
- Đảm bảo encoding nhất quán trong toàn bộ ứng dụng
- Tự động cấu hình LoadOptions và SaveOptions phù hợp

### 2. Cập nhật ProductService
Đã cập nhật `ProductService.cs` để:
- Sử dụng `ExcelHelper.CreateWorkbook()` thay vì tạo workbook trực tiếp
- Sử dụng `ExcelHelper.GetCellStringValue()` để đọc dữ liệu từ cell
- Sử dụng `ExcelHelper.SetCellValue()` để ghi dữ liệu vào cell
- Sử dụng `ExcelHelper.SaveToByteArray()` để lưu workbook

### 3. Cập nhật ProductController
Đã cập nhật `ProductController.cs` để:
- Sử dụng `ExcelHelper.GetContentDispositionHeader()` cho header phù hợp
- Sử dụng `ExcelHelper.GetExcelContentType()` cho Content-Type đúng

### 4. Cập nhật GenerateProductTemplate Script
Đã cập nhật script tạo template tĩnh để sử dụng ExcelHelper.

## Các tính năng của ExcelHelper

### LoadOptions và SaveOptions
```csharp
// Tự động cấu hình cho UTF-8
var loadOptions = ExcelHelper.GetDefaultLoadOptions();
var saveOptions = ExcelHelper.GetDefaultSaveOptions();
```

### Tạo Workbook
```csharp
// Tạo workbook mới
var workbook = ExcelHelper.CreateWorkbook();

// Tạo workbook từ stream
var workbook = ExcelHelper.CreateWorkbook(stream);
```

### Xử lý Cell Value
```csharp
// Đọc giá trị với encoding đúng
string value = ExcelHelper.GetCellStringValue(cell);

// Ghi giá trị với encoding đúng
ExcelHelper.SetCellValue(cell, "Tiếng Việt có dấu");
```

### Lưu File
```csharp
// Lưu thành byte array
byte[] data = ExcelHelper.SaveToByteArray(workbook);

// Lưu thành file
ExcelHelper.SaveToFile(workbook, filePath);
```

### HTTP Headers
```csharp
// Content-Disposition với UTF-8 filename
string header = ExcelHelper.GetContentDispositionHeader(fileName);

// Content-Type cho Excel
string contentType = ExcelHelper.GetExcelContentType();
```

## Lợi ích

### 1. Hiển thị đúng ký tự tiếng Việt
- Tất cả ký tự có dấu hiển thị chính xác
- Không bị lỗi encoding khi mở file

### 2. Tính nhất quán
- Tất cả file Excel trong hệ thống sử dụng cùng cấu hình
- Dễ dàng bảo trì và cập nhật

### 3. Hiệu suất
- Tối ưu hóa việc tạo và đọc file Excel
- Giảm thiểu overhead không cần thiết

### 4. Khả năng mở rộng
- Dễ dàng thêm tính năng mới cho Excel
- Có thể áp dụng cho các entity khác

## Cách sử dụng

### Tạo file Excel mới
```csharp
using (var workbook = ExcelHelper.CreateWorkbook())
{
    var worksheet = workbook.Worksheets.Add("Sheet1");
    ExcelHelper.SetCellValue(worksheet.Cell(1, 1), "Tiếng Việt");
    return ExcelHelper.SaveToByteArray(workbook);
}
```

### Đọc file Excel
```csharp
using (var workbook = ExcelHelper.CreateWorkbook(stream))
{
    var worksheet = workbook.Worksheet(1);
    string value = ExcelHelper.GetCellStringValue(worksheet.Cell(1, 1));
}
```

### Trả về file Excel từ Controller
```csharp
var bytes = await service.GenerateExcelFile();
var fileName = "file.xlsx";

Response.Headers.Add("Content-Disposition", 
    ExcelHelper.GetContentDispositionHeader(fileName));

return File(bytes, ExcelHelper.GetExcelContentType(), fileName);
```

## Kiểm tra

### 1. Tạo file Excel mới
- Download template từ `/Product/DownloadTemplate`
- Kiểm tra hiển thị ký tự tiếng Việt

### 2. Import file Excel
- Tạo file Excel với dữ liệu tiếng Việt
- Import qua `/Product/ImportExcel`
- Kiểm tra dữ liệu được lưu đúng

### 3. Tương thích
- Kiểm tra với Microsoft Excel
- Kiểm tra với LibreOffice Calc
- Kiểm tra với Google Sheets

## Lưu ý
- Luôn sử dụng ExcelHelper cho tất cả thao tác Excel
- Không tạo workbook trực tiếp từ ClosedXML
- Kiểm tra encoding khi có vấn đề hiển thị

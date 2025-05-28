# Hướng dẫn sử dụng chức năng xuất hóa đơn cho Purchase Order

## Tổng quan

Chức năng xuất hóa đơn cho Purchase Order cho phép tạo và xuất hóa đơn từ các đơn đặt hàng đã có trong hệ thống. Hệ thống hỗ trợ xuất hóa đơn ở nhiều định dạng khác nhau.

## Các định dạng hỗ trợ

1. **PDF** - Định dạng in ấn chuyên nghiệp (hiện tại xuất HTML để in)
2. **Excel** - Định dạng bảng tính với đầy đủ thông tin
3. **Word** - Định dạng văn bản có thể chỉnh sửa

## API Endpoints

### 1. Tạo hóa đơn từ Purchase Order
```
GET /PurchaseOrder/GenerateInvoice?purchaseOrderId={id}
```
**Mô tả**: Tạo dữ liệu hóa đơn từ Purchase Order
**Tham số**: 
- `purchaseOrderId` (int): ID của Purchase Order

**Response**: 
```json
{
  "isSuccess": true,
  "data": {
    "invoiceCode": "INV_000001",
    "purchaseOrderId": 1,
    "purchaseOrderCode": "PO_000001",
    "invoiceDate": "2024-01-15T00:00:00",
    "dueDate": "2024-02-14T00:00:00",
    "customer": {
      "name": "Tên khách hàng",
      "address": "Địa chỉ",
      "taxCode": "Mã số thuế"
    },
    "items": [...],
    "totalAmount": 1000000
  }
}
```

### 2. Xuất hóa đơn PDF
```
GET /PurchaseOrder/ExportInvoicePdf?purchaseOrderId={id}
```
**Mô tả**: Xuất hóa đơn dạng PDF (HTML để in)
**Response**: File HTML có thể in thành PDF

### 3. Xuất hóa đơn Excel
```
GET /PurchaseOrder/ExportInvoiceExcel?purchaseOrderId={id}
```
**Mô tả**: Xuất hóa đơn dạng Excel
**Response**: File Excel (.xlsx)

### 4. Xuất hóa đơn Word
```
GET /PurchaseOrder/ExportInvoiceWord?purchaseOrderId={id}
```
**Mô tả**: Xuất hóa đơn dạng Word
**Response**: File Word (.docx)

## Cách sử dụng

### 1. Từ giao diện web
```javascript
// Xuất PDF
window.open('/PurchaseOrder/ExportInvoicePdf?purchaseOrderId=1');

// Xuất Excel
window.open('/PurchaseOrder/ExportInvoiceExcel?purchaseOrderId=1');

// Xuất Word
window.open('/PurchaseOrder/ExportInvoiceWord?purchaseOrderId=1');
```

### 2. Từ JavaScript với AJAX
```javascript
// Tạo hóa đơn
async function generateInvoice(purchaseOrderId) {
    const response = await fetch(`/PurchaseOrder/GenerateInvoice?purchaseOrderId=${purchaseOrderId}`);
    const result = await response.json();
    
    if (result.isSuccess) {
        console.log('Invoice generated:', result.data.invoiceCode);
        return result.data;
    } else {
        console.error('Failed to generate invoice:', result.errorMessageList);
    }
}

// Xuất Excel
async function exportInvoiceExcel(purchaseOrderId) {
    const response = await fetch(`/PurchaseOrder/ExportInvoiceExcel?purchaseOrderId=${purchaseOrderId}`);
    const blob = await response.blob();
    
    // Tạo link download
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Invoice_${Date.now()}.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}
```

## Cấu trúc dữ liệu hóa đơn

### InvoiceViewModel
```csharp
public class InvoiceViewModel
{
    public string InvoiceCode { get; set; }           // Mã hóa đơn
    public int PurchaseOrderId { get; set; }          // ID Purchase Order
    public string PurchaseOrderCode { get; set; }     // Mã Purchase Order
    public DateTime InvoiceDate { get; set; }         // Ngày hóa đơn
    public DateTime? DueDate { get; set; }            // Hạn thanh toán
    public CustomerInvoiceInfo Customer { get; set; }  // Thông tin khách hàng
    public CompanyInvoiceInfo Company { get; set; }    // Thông tin công ty
    public List<InvoiceItemViewModel> Items { get; set; } // Danh sách sản phẩm
    public decimal Subtotal { get; set; }             // Tổng tiền chưa thuế
    public decimal TaxAmount { get; set; }            // Tiền thuế
    public decimal TotalAmount { get; set; }          // Tổng cộng
    public string Notes { get; set; }                 // Ghi chú
    public string PaymentTerms { get; set; }          // Điều khoản thanh toán
}
```

### InvoiceItemViewModel
```csharp
public class InvoiceItemViewModel
{
    public int SequenceNumber { get; set; }           // STT
    public string ProductCode { get; set; }           // Mã sản phẩm
    public string ProductName { get; set; }           // Tên sản phẩm
    public string Unit { get; set; }                  // Đơn vị tính
    public decimal Quantity { get; set; }             // Số lượng
    public decimal UnitPrice { get; set; }            // Đơn giá
    public decimal TaxRate { get; set; }              // Thuế suất (%)
    public decimal AmountBeforeTax { get; set; }      // Tiền chưa thuế
    public decimal TaxAmount { get; set; }            // Tiền thuế
    public decimal TotalAmount { get; set; }          // Thành tiền
    public string Notes { get; set; }                 // Ghi chú
}
```

## Tùy chỉnh template

### 1. Template HTML
File template HTML có thể tùy chỉnh tại: `/wwwroot/templates/invoice_template.html`

### 2. Thông tin công ty
Thông tin công ty mặc định có thể thay đổi trong `CompanyInvoiceInfo`:
```csharp
public class CompanyInvoiceInfo
{
    public string Name { get; set; } = "CÔNG TY TNHH TASIN";
    public string Address { get; set; } = "Địa chỉ công ty";
    public string Phone { get; set; } = "Số điện thoại";
    public string Email { get; set; } = "Email công ty";
    public string TaxCode { get; set; } = "Mã số thuế";
    public string BankAccount { get; set; } = "Số tài khoản";
    public string BankName { get; set; } = "Tên ngân hàng";
}
```

## Test chức năng

### Endpoint test
```
GET /Test/TestInvoice?purchaseOrderId=1
```

Endpoint này sẽ test toàn bộ chức năng tạo hóa đơn và trả về kết quả test.

## Lưu ý

1. **Mã hóa đơn**: Được tự động tạo theo quy tắc `INV_XXXXXX`
2. **Hạn thanh toán**: Mặc định là 30 ngày từ ngày tạo hóa đơn
3. **Thuế suất**: Lấy từ thông tin sản phẩm trong Purchase Order
4. **PDF**: Hiện tại xuất HTML để in, có thể tích hợp thư viện PDF chuyên dụng
5. **Encoding**: Hỗ trợ đầy đủ tiếng Việt có dấu

## Mở rộng

### Tích hợp PDF library
Để tạo PDF thực sự, có thể tích hợp:
- **DinkToPdf**: Chuyển HTML thành PDF
- **iTextSharp**: Tạo PDF từ code
- **PuppeteerSharp**: Sử dụng Chrome headless

### Lưu trữ hóa đơn
Có thể mở rộng để lưu trữ hóa đơn vào database với bảng `Invoice` và `Invoice_Item`.

### Email hóa đơn
Có thể tích hợp gửi hóa đơn qua email cho khách hàng.

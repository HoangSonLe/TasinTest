# Hướng dẫn sử dụng Template HTML cho Hóa đơn

## Tổng quan

Hệ thống hỗ trợ sử dụng template HTML động để tạo hóa đơn với khả năng tùy chỉnh cao. Template sử dụng cú pháp đơn giản để truyền biến và xử lý logic điều kiện.

## File Template

**File chính**: `wwwroot/templates/invoice_template_dynamic.html`

## Cú pháp Template

### 1. Biến đơn giản
```html
{{VariableName}}
```
**Ví dụ**:
```html
<h1>{{InvoiceCode}}</h1>
<p>{{CustomerName}}</p>
<span>{{TotalAmount}}</span>
```

### 2. Khối điều kiện (Conditional Blocks)
```html
{{#FieldName}}
  Nội dung hiển thị khi FieldName có giá trị
  <p>{{FieldName}}</p>
{{/FieldName}}
```

**Ví dụ**:
```html
{{#DueDate}}
<div class="info-row">
    <strong>Hạn thanh toán:</strong>
    <span>{{DueDate}}</span>
</div>
{{/DueDate}}

{{#CustomerPhone}}
<div class="info-row">
    <strong>Điện thoại:</strong>
    <span>{{CustomerPhone}}</span>
</div>
{{/CustomerPhone}}
```

### 3. Vòng lặp (Loops)
```html
{{#InvoiceItems}}
<tr>
    <td>{{SequenceNumber}}</td>
    <td>{{ProductCode}}</td>
    <td>{{ProductName}}</td>
    <td>{{Quantity}}</td>
    <td>{{UnitPrice}}</td>
    <td>{{TotalAmount}}</td>
</tr>
{{/InvoiceItems}}
```

## Danh sách biến có sẵn

### Thông tin hóa đơn
- `{{InvoiceCode}}` - Mã hóa đơn
- `{{PurchaseOrderCode}}` - Mã đơn hàng
- `{{InvoiceDate}}` - Ngày hóa đơn (đã format)
- `{{DueDate}}` - Hạn thanh toán (tùy chọn)

### Thông tin công ty
- `{{CompanyName}}` - Tên công ty
- `{{CompanyAddress}}` - Địa chỉ công ty
- `{{CompanyPhone}}` - Số điện thoại
- `{{CompanyEmail}}` - Email công ty
- `{{CompanyTaxCode}}` - Mã số thuế
- `{{CompanyBankAccount}}` - Số tài khoản (tùy chọn)
- `{{CompanyBankName}}` - Tên ngân hàng (tùy chọn)

### Thông tin khách hàng
- `{{CustomerName}}` - Tên khách hàng
- `{{CustomerCode}}` - Mã khách hàng
- `{{CustomerAddress}}` - Địa chỉ khách hàng
- `{{CustomerPhone}}` - Số điện thoại (tùy chọn)
- `{{CustomerEmail}}` - Email (tùy chọn)
- `{{CustomerTaxCode}}` - Mã số thuế (tùy chọn)

### Thông tin tài chính
- `{{Subtotal}}` - Tổng tiền chưa thuế (đã format VNĐ)
- `{{TaxAmount}}` - Tiền thuế (đã format VNĐ)
- `{{TotalAmount}}` - Tổng cộng (đã format VNĐ)
- `{{TotalAmountInWords}}` - Tổng tiền bằng chữ

### Thông tin khác
- `{{PaymentTerms}}` - Điều khoản thanh toán (tùy chọn)
- `{{Notes}}` - Ghi chú (tùy chọn)

### Danh sách sản phẩm (InvoiceItems)
Trong vòng lặp `{{#InvoiceItems}}...{{/InvoiceItems}}`:
- `{{SequenceNumber}}` - STT
- `{{ProductCode}}` - Mã sản phẩm
- `{{ProductName}}` - Tên sản phẩm
- `{{Unit}}` - Đơn vị tính
- `{{Quantity}}` - Số lượng (đã format)
- `{{UnitPrice}}` - Đơn giá (đã format VNĐ)
- `{{TaxRate}}` - Thuế suất (%)
- `{{TotalAmount}}` - Thành tiền (đã format VNĐ)

## Ví dụ sử dụng

### 1. Hiển thị thông tin tùy chọn
```html
<!-- Chỉ hiển thị khi có số điện thoại -->
{{#CustomerPhone}}
<div class="contact-info">
    <strong>Liên hệ:</strong> {{CustomerPhone}}
</div>
{{/CustomerPhone}}

<!-- Chỉ hiển thị khi có ghi chú -->
{{#Notes}}
<div class="notes-section">
    <h4>Ghi chú:</h4>
    <p>{{Notes}}</p>
</div>
{{/Notes}}
```

### 2. Bảng sản phẩm
```html
<table class="invoice-table">
    <thead>
        <tr>
            <th>STT</th>
            <th>Sản phẩm</th>
            <th>Số lượng</th>
            <th>Đơn giá</th>
            <th>Thành tiền</th>
        </tr>
    </thead>
    <tbody>
        {{#InvoiceItems}}
        <tr>
            <td class="text-center">{{SequenceNumber}}</td>
            <td>
                <strong>{{ProductName}}</strong><br>
                <small>Mã: {{ProductCode}}</small>
            </td>
            <td class="text-right">{{Quantity}} {{Unit}}</td>
            <td class="text-right">{{UnitPrice}}</td>
            <td class="text-right">{{TotalAmount}}</td>
        </tr>
        {{/InvoiceItems}}
    </tbody>
</table>
```

### 3. Thông tin ngân hàng (tùy chọn)
```html
{{#CompanyBankAccount}}
<div class="bank-info">
    <h4>Thông tin chuyển khoản:</h4>
    <p>STK: {{CompanyBankAccount}}</p>
    <p>Ngân hàng: {{CompanyBankName}}</p>
</div>
{{/CompanyBankAccount}}
```

## Tùy chỉnh CSS

Template đã bao gồm CSS hoàn chỉnh với:
- Responsive design
- Print-friendly styles
- Professional formatting
- Vietnamese font support

### Các class CSS có sẵn:
- `.invoice-container` - Container chính
- `.invoice-header` - Header hóa đơn
- `.company-info` - Thông tin công ty
- `.customer-info` - Thông tin khách hàng
- `.invoice-table` - Bảng sản phẩm
- `.invoice-summary` - Tổng kết thanh toán
- `.signature-section` - Khu vực ký tên
- `.text-left`, `.text-center`, `.text-right` - Căn chỉnh text
- `.currency` - Định dạng tiền tệ
- `.highlight` - Làm nổi bật

## API Endpoints

### 1. Xem trước hóa đơn
```
GET /PurchaseOrder/PreviewInvoice?purchaseOrderId=1
```
Hiển thị HTML trực tiếp trong trình duyệt để xem trước.

### 2. Xuất PDF
```
GET /PurchaseOrder/ExportInvoicePdf?purchaseOrderId=1
```

### 3. Xuất Excel
```
GET /PurchaseOrder/ExportInvoiceExcel?purchaseOrderId=1
```

### 4. Xuất Word
```
GET /PurchaseOrder/ExportInvoiceWord?purchaseOrderId=1
```

## Cách tùy chỉnh Template

### 1. Chỉnh sửa trực tiếp
Mở file `wwwroot/templates/invoice_template_dynamic.html` và chỉnh sửa:
- HTML structure
- CSS styles
- Thêm/bớt trường thông tin
- Thay đổi layout

### 2. Thêm trường mới
1. Cập nhật `InvoiceViewModel` để thêm property mới
2. Cập nhật `TemplateHelper.ReplaceTemplateVariables()` để xử lý biến mới
3. Thêm `{{NewField}}` vào template HTML

### 3. Thêm logic điều kiện mới
1. Sử dụng cú pháp `{{#FieldName}}...{{/FieldName}}`
2. Cập nhật `TemplateHelper` để xử lý conditional block mới

## Lưu ý quan trọng

1. **Encoding**: Template hỗ trợ đầy đủ UTF-8 và tiếng Việt
2. **Fallback**: Nếu template không tồn tại, hệ thống sẽ dùng phương pháp tạo HTML cũ
3. **Performance**: Template được cache và xử lý nhanh
4. **Security**: Tất cả dữ liệu được escape để tránh XSS
5. **Print-ready**: Template đã tối ưu cho in ấn

## Troubleshooting

### Template không hiển thị
- Kiểm tra file `invoice_template_dynamic.html` có tồn tại
- Kiểm tra quyền đọc file
- Xem log để biết lỗi cụ thể

### Biến không được thay thế
- Kiểm tra tên biến có đúng không (case-sensitive)
- Kiểm tra dữ liệu có null không
- Kiểm tra cú pháp `{{VariableName}}`

### CSS không hiển thị đúng
- Kiểm tra CSS syntax
- Test trên trình duyệt khác nhau
- Kiểm tra print preview

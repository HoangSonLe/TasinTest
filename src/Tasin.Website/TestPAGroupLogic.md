# Test Logic cho PAGroup với Parent-Child Products và LossRate

## Tóm tắt thay đổi

Đã cập nhật logic tổng hợp PAGroup để:

### 1. Chuyển đổi sản phẩm con thành sản phẩm cha
- Khi tổng hợp đơn hàng, nếu sản phẩm có `ParentID` (là sản phẩm con), sẽ chuyển thành sản phẩm cha
- Sử dụng `TargetProductId = product?.ParentID ?? item.Product_ID`

### 2. Tính toán số lượng với LossRate
- Khi chuyển từ sản phẩm con sang sản phẩm cha, áp dụng công thức:
  ```csharp
  adjustedQuantity = (100 + product.LossRate.Value) * item.Quantity / 100
  ```

### 3. Cập nhật Product-Vendor relationships
- Sử dụng target product IDs (parent product IDs) để lấy vendor relationships
- Đảm bảo vendor mapping đúng với parent products

## Test Cases

### Test Case 1: Sản phẩm không có parent
**Input:**
- Product A (ID: 1, ParentID: null, LossRate: null)
- Quantity: 100

**Expected Output:**
- TargetProductId: 1
- AdjustedQuantity: 100

### Test Case 2: Sản phẩm con có parent và LossRate
**Input:**
- Product B (ID: 2, ParentID: 1, LossRate: 5%)
- Quantity: 100

**Expected Output:**
- TargetProductId: 1 (parent product)
- AdjustedQuantity: 105 (100 * (100 + 5) / 100)

### Test Case 3: Sản phẩm con có parent nhưng không có LossRate
**Input:**
- Product C (ID: 3, ParentID: 1, LossRate: null)
- Quantity: 100

**Expected Output:**
- TargetProductId: 1 (parent product)
- AdjustedQuantity: 100 (không thay đổi)

### Test Case 4: Tổng hợp nhiều sản phẩm con cùng parent
**Input:**
- Product B (ID: 2, ParentID: 1, LossRate: 5%), Quantity: 100
- Product C (ID: 3, ParentID: 1, LossRate: 10%), Quantity: 50

**Expected Output:**
- TargetProductId: 1 (cùng parent)
- TotalQuantity: 160 (105 + 55)

## Files đã thay đổi

1. **`PurchaseAgreementService.cs`**:
   - `GetEditablePAGroupPreview()`: Thêm logic transformation
   - `GetPAGroupPreview()`: Thêm logic transformation
   - `CreatePAGroup()`: Cập nhật để sử dụng target products

## Cách test

1. Tạo sản phẩm cha (Material) với ID = 1
2. Tạo sản phẩm con với ParentID = 1 và LossRate = 5%
3. Tạo Purchase Order với sản phẩm con
4. Gọi `GetEditablePAGroupPreview()` và kiểm tra:
   - Product_ID trong mapping phải là ID của parent product
   - TotalQuantity phải được tính với loss rate

## Lưu ý

- Logic chỉ áp dụng khi sản phẩm có ParentID (là sản phẩm con)
- LossRate chỉ được áp dụng khi chuyển từ child sang parent
- Vendor relationships được lấy dựa trên parent product IDs

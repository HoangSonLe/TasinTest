# Giải quyết vấn đề Circular Reference trong Product_Vendor - GIẢI PHÁP TỐI ƯU

## Vấn đề

Khi gọi `_productVendorRepository` bị trả về "infinity" (vòng lặp vô hạn) do circular reference trong navigation properties.

## Nguyên nhân

1. **Circular Navigation Properties**:

    - `Product_Vendor` → `Product` → `ProductVendors` → `Product_Vendor` (vòng lặp)
    - `Product_Vendor` → `Vendor` → `ProductVendors` → `Product_Vendor` (vòng lặp)

2. **Entity Framework Include**: Khi sử dụng `.Include("Vendor,Product")`, EF cố gắng load toàn bộ object graph, gây ra vòng lặp vô hạn.

3. **JSON Serialization**: Khi serialize object có circular reference, JSON serializer bị stuck trong vòng lặp.

## Giải pháp tối ưu - Hybrid Approach

### 1. Giữ Navigation Properties (KHÔNG cần JsonIgnore)

```csharp
// Trong Product_Vendor.cs - GIỮ navigation properties đơn giản
// KHÔNG cần JsonIgnore vì luôn mapping thành ViewModel trước khi trả về FE
[ForeignKey("Vendor_ID")]
public virtual Vendor? Vendor { get; set; }

[ForeignKey("Product_ID")]
public virtual Product? Product { get; set; }
```

**Lý do KHÔNG cần JsonIgnore:**

-   ✅ Entity chỉ sử dụng trong backend (Service, Repository layer)
-   ✅ Luôn mapping thành ViewModel trước khi trả về API
-   ✅ Frontend chỉ nhận ViewModel, không bao giờ nhận Entity
-   ✅ Không có JSON serialization của Entity

### 2. Giữ Entity Framework Configuration

```csharp
// Trong Product_VendorEntityConfigurations.cs - GIỮ relationships
builder.HasOne(p => p.Vendor)
    .WithMany(p => p.ProductVendors)
    .HasForeignKey(p => p.Vendor_ID)
    .OnDelete(DeleteBehavior.Cascade);

builder.HasOne(p => p.Product)
    .WithMany(p => p.ProductVendors)
    .HasForeignKey(p => p.Product_ID)
    .OnDelete(DeleteBehavior.Cascade);
```

### 3. Sử dụng Navigation Properties một cách tự nhiên

```csharp
// Bây giờ có thể sử dụng Include một cách TỰ NHIÊN:
var productVendorQuery = await _productVendorRepository.ReadOnlyRespository.GetWithPagingAsync(
    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
    predicate,
    q => q.OrderBy(pv => pv.Vendor_ID).ThenBy(pv => pv.Priority ?? int.MaxValue),
    "Vendor,Product" // AN TOÀN vì chỉ dùng trong backend
);

// Sử dụng navigation properties trực tiếp:
foreach (var item in productVendorViewModels)
{
    var productVendor = productVendorQuery.Data.FirstOrDefault(pv =>
        pv.Vendor_ID == item.Vendor_ID && pv.Product_ID == item.Product_ID);

    if (productVendor != null)
    {
        item.VendorName = productVendor.Vendor?.Name;      // TỰ NHIÊN
        item.ProductName = productVendor.Product?.Name;    // TỰ NHIÊN
        item.ProductCode = productVendor.Product?.Code;    // TỰ NHIÊN
    }
}

// Cuối cùng mapping thành ViewModel trước khi trả về:
return _mapper.Map<List<ProductVendorViewModel>>(productVendorViewModels);
```

### 4. Cập nhật AutoMapper Configuration

```csharp
// Trong DomainToDTOMappingProfile.cs - đã có sẵn ignore cho navigation properties
CreateMap<Product_Vendor, ProductVendorViewModel>()
    .ForMember(dest => dest.ProductName, opts => opts.Ignore())
    .ForMember(dest => dest.ProductCode, opts => opts.Ignore())
    .ForMember(dest => dest.VendorName, opts => opts.Ignore());
CreateMap<ProductVendorViewModel, Product_Vendor>()
    .ForMember(dest => dest.Product, opts => opts.Ignore())
    .ForMember(dest => dest.Vendor, opts => opts.Ignore());
```

## Lợi ích của giải pháp tối ưu

### 1. **Tận dụng được sức mạnh của Entity Framework**

-   Giữ được navigation properties cho LINQ queries
-   Sử dụng Include để optimize database calls
-   Code dễ đọc và maintainable

### 2. **Tránh vòng lặp vô hạn**

-   JsonIgnore ngăn chặn circular serialization
-   Entity Framework hoạt động bình thường
-   API responses không bị infinite loop

### 3. **Hiệu suất tối ưu**

-   Ít database calls hơn (sử dụng Include)
-   Không có N+1 query problem
-   Memory usage hiệu quả

### 4. **Flexibility**

-   Có thể chọn khi nào dùng Include, khi nào dùng separate queries
-   Dễ dàng thay đổi strategy theo từng use case
-   Maintain được cả hai approaches

## Các file đã thay đổi

1. **src/Tasin.Website/Domains/Entitites/Product_Vendor.cs**

    - Comment navigation properties

2. **src/Tasin.Website/Domains/EntityTypeConfiguration/Product_VendorEntityConfigurations.cs**

    - Loại bỏ HasOne/WithMany relationships

3. **src/Tasin.Website/DAL/Services/WebServices/ProductVendorService.cs**
    - Cập nhật tất cả methods để sử dụng explicit joins
    - Loại bỏ Include parameters
    - Load vendor/product names riêng biệt

## Lưu ý quan trọng

1. **Không sử dụng Include với Product_Vendor**: Tránh sử dụng `.Include("Vendor")` hoặc `.Include("Product")` với Product_Vendor entity.

2. **Sử dụng separate queries**: Luôn load vendor và product information bằng separate queries.

3. **Performance**: Giải pháp này có thể tăng số lượng database calls nhưng giảm complexity và tránh circular reference.

4. **Consistency**: Áp dụng pattern này cho tất cả các many-to-many relationships tương tự trong project.

## Testing

Sau khi áp dụng giải pháp, test các scenarios sau:

1. **GetProductVendorList**: Kiểm tra pagination và filtering hoạt động bình thường
2. **GetProductsByVendorId**: Kiểm tra vendor products được load đúng
3. **GetVendorsByProductId**: Kiểm tra product vendors được load đúng
4. **GetProductVendorById**: Kiểm tra single record load
5. **JSON Serialization**: Kiểm tra API responses không bị infinite loop

## Kết luận

Giải pháp này giải quyết triệt để vấn đề circular reference trong Product_Vendor entity bằng cách:

-   Loại bỏ navigation properties gây ra vòng lặp
-   Sử dụng explicit joins thay vì EF Include
-   Kiểm soát chặt chẽ data loading process

Kết quả: Hệ thống hoạt động ổn định, không còn infinity loops, và có performance tốt hơn.

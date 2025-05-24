-- Xóa dữ liệu các bảng theo thứ tự tránh lỗi khóa ngoại
DELETE FROM "Product_Vendor";
DELETE FROM "Product";
DELETE FROM "Customer";
DELETE FROM "Vendor";
DELETE FROM "Material";
DELETE FROM "SpecialProductTaxRate";
DELETE FROM "ProcessingType";
DELETE FROM "Category";
DELETE FROM "Unit";

-- Reset lại sequence cho các bảng (ID chạy từ 1 trở lại)
ALTER SEQUENCE "Vendor_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "Unit_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "Category_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "ProcessingType_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "Customer_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "SpecialProductTaxRate_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "Material_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "Product_ID_seq" RESTART WITH 1;

-- INSERT dữ liệu mẫu

-- Đơn vị tính (20 dòng)
INSERT INTO "Unit" ("Code", "Name", "NameNonUnicode", "Status", "CreatedBy", "UpdatedBy") VALUES
('UNIT01', 'Kilôgam', 'Kilogram', '0', 1, 1),
('UNIT02', 'Lít', 'Lit', '0', 1, 1),
('UNIT03', 'Thùng', 'Thung', '0', 1, 1),
('UNIT04', 'Hộp', 'Hop', '0', 1, 1),
('UNIT05', 'Gói', 'Goi', '0', 1, 1),
('UNIT06', 'Chai', 'Chai', '0', 1, 1),
('UNIT07', 'Cái', 'Cai', '0', 1, 1),
('UNIT08', 'Bao', 'Bao', '0', 1, 1),
('UNIT09', 'Lon', 'Lon', '0', 1, 1),
('UNIT10', 'Cân', 'Can', '0', 1, 1),
('UNIT11', 'Túi', 'Tui', '0', 1, 1),
('UNIT12', 'Viên', 'Vien', '0', 1, 1),
('UNIT13', 'Thùng carton', 'Thung carton', '0', 1, 1),
('UNIT14', 'Kg', 'Kg', '0', 1, 1),
('UNIT15', 'm3', 'm3', '0', 1, 1),
('UNIT16', 'Cây', 'Cay', '0', 1, 1),
('UNIT17', 'Gói nhỏ', 'Goi nho', '0', 1, 1),
('UNIT18', 'Hộp lớn', 'Hop lon', '0', 1, 1),
('UNIT19', 'Lạng', 'Lang', '0', 1, 1),
('UNIT20', 'Thùng nhỏ', 'Thung nho', '0', 1, 1);

-- Danh mục sản phẩm (20 dòng)
INSERT INTO "Category" ("Code", "Name", "NameNonUnicode", "Status", "CreatedBy", "UpdatedBy") VALUES
('CAT01', 'Thực phẩm tươi sống', 'Thuc pham tuoi song', '0', 1, 1),
('CAT02', 'Thực phẩm chế biến', 'Thuc pham che bien', '0', 1, 1),
('CAT03', 'Đồ uống', 'Do uong', '0', 1, 1),
('CAT04', 'Gia vị', 'Gia vi', '0', 1, 1),
('CAT05', 'Đồ ăn nhanh', 'Do an nhanh', '0', 1, 1),
('CAT06', 'Sản phẩm làm bánh', 'San pham lam banh', '0', 1, 1),
('CAT07', 'Rau củ quả', 'Rau cu qua', '0', 1, 1),
('CAT08', 'Thực phẩm đông lạnh', 'Thuc pham dong lanh', '0', 1, 1),
('CAT09', 'Hải sản', 'Hai san', '0', 1, 1),
('CAT10', 'Sữa và sản phẩm từ sữa', 'Sua va san pham tu sua', '0', 1, 1),
('CAT11', 'Thịt', 'Thit', '0', 1, 1),
('CAT12', 'Ngũ cốc', 'Ngu coc', '0', 1, 1),
('CAT13', 'Bánh kẹo', 'Banh keo', '0', 1, 1),
('CAT14', 'Đồ uống có cồn', 'Do uong co con', '0', 1, 1),
('CAT15', 'Nước giải khát', 'Nuoc giai khat', '0', 1, 1),
('CAT16', 'Đồ hộp', 'Do hop', '0', 1, 1),
('CAT17', 'Thực phẩm hữu cơ', 'Thuc pham huu co', '0', 1, 1),
('CAT18', 'Đồ ăn nhẹ', 'Do an nhe', '0', 1, 1),
('CAT19', 'Nguyên liệu chế biến', 'Nguyen lieu che bien', '0', 1, 1),
('CAT20', 'Đồ uống đóng chai', 'Do uong dong chai', '0', 1, 1);

-- Kiểu chế biến (20 dòng)
INSERT INTO "ProcessingType" ("Code", "Name", "NameNonUnicode", "Status", "CreatedBy", "UpdatedBy") VALUES
('PROC01', 'Đông lạnh', 'Dong lanh', '0', 1, 1),
('PROC02', 'Đóng hộp', 'Dong hop', '0', 1, 1),
('PROC03', 'Sấy khô', 'Say kho', '0', 1, 1),
('PROC04', 'Tươi sống', 'Tuoi song', '0', 1, 1),
('PROC05', 'Hấp', 'Hap', '0', 1, 1),
('PROC06', 'Nướng', 'Nuong', '0', 1, 1),
('PROC07', 'Luộc', 'Luoc', '0', 1, 1),
('PROC08', 'Xông khói', 'Xong khoi', '0', 1, 1),
('PROC09', 'Rán', 'Ran', '0', 1, 1),
('PROC10', 'Kho', 'Kho', '0', 1, 1),
('PROC11', 'Làm lạnh', 'Lam lanh', '0', 1, 1),
('PROC12', 'Đóng gói', 'Dong goi', '0', 1, 1),
('PROC13', 'Làm mềm', 'Lam mem', '0', 1, 1),
('PROC14', 'Ủ men', 'U men', '0', 1, 1),
('PROC15', 'Cắt nhỏ', 'Cat nho', '0', 1, 1),
('PROC16', 'Trộn', 'Tron', '0', 1, 1),
('PROC17', 'Ép', 'Ep', '0', 1, 1),
('PROC18', 'Chưng cất', 'Chung cat', '0', 1, 1),
('PROC19', 'Làm đông', 'Lam dong', '0', 1, 1),
('PROC20', 'Ủ lạnh', 'U lanh', '0', 1, 1);

-- Thuế sản phẩm đặc biệt (20 dòng)
INSERT INTO "SpecialProductTaxRate" ("Code", "Name", "NameNonUnicode", "Status", "CreatedBy", "UpdatedBy") VALUES
('TAX01', 'Thuế thực phẩm nhập khẩu', 'Thue thuc pham nhap khau', '0', 1, 1),
('TAX02', 'Thuế vệ sinh an toàn', 'Thue ve sinh an toan', '0', 1, 1),
('TAX03', 'Thuế hàng xa xỉ', 'Thue hang xa xi', '0', 1, 1),
('TAX04', 'Thuế bảo vệ sức khỏe', 'Thue bao ve suc khoe', '0', 1, 1),
('TAX05', 'Thuế rượu bia', 'Thue ruou bia', '0', 1, 1),
('TAX06', 'Thuế thuốc lá', 'Thue thuoc la', '0', 1, 1),
('TAX07', 'Thuế môi trường', 'Thue moi truong', '0', 1, 1),
('TAX08', 'Thuế dầu mỏ', 'Thue dau mo', '0', 1, 1),
('TAX09', 'Thuế hàng điện tử', 'Thue hang dien tu', '0', 1, 1),
('TAX10', 'Thuế nhập khẩu đặc biệt', 'Thue nhap khau dac biet', '0', 1, 1),
('TAX11', 'Thuế bán hàng', 'Thue ban hang', '0', 1, 1),
('TAX12', 'Thuế tiêu thụ đặc biệt', 'Thue tieu thu dac biet', '0', 1, 1),
('TAX13', 'Thuế nhập khẩu ưu đãi', 'Thue nhap khau uu dai', '0', 1, 1),
('TAX14', 'Thuế chống bán phá giá', 'Thue chong ban pha gia', '0', 1, 1),
('TAX15', 'Thuế chống trợ cấp', 'Thue chong tro cap', '0', 1, 1),
('TAX16', 'Thuế môi trường phát thải', 'Thue moi truong phat thai', '0', 1, 1),
('TAX17', 'Thuế khai thác tài nguyên', 'Thue khai thac tai nguyen', '0', 1, 1),
('TAX18', 'Thuế đóng góp xã hội', 'Thue dong gop xa hoi', '0', 1, 1),
('TAX19', 'Thuế phí hải quan', 'Thue phi hai quan', '0', 1, 1),
('TAX20', 'Thuế bảo hiểm', 'Thue bao hiem', '0', 1, 1);

-- Chất liệu (20 dòng)
INSERT INTO "Material" ("Code", "Name", "NameNonUnicode", "Status", "CreatedBy", "UpdatedBy") VALUES
('MAT01', 'Thịt heo', 'Thit heo', '0', 1, 1),
('MAT02', 'Cá hồi', 'Ca hoi', '0', 1, 1),
('MAT03', 'Gạo', 'Gao', '0', 1, 1),
('MAT04', 'Muối', 'Muoi', '0', 1, 1),
('MAT05', 'Đường', 'Duong', '0', 1, 1),
('MAT06', 'Bơ', 'Bo', '0', 1, 1),
('MAT07', 'Sữa', 'Sua', '0', 1, 1),
('MAT08', 'Bột mì', 'Bot mi', '0', 1, 1),
('MAT09', 'Dầu ăn', 'Dau an', '0', 1, 1),
('MAT10', 'Tiêu', 'Tieu', '0', 1, 1),
('MAT11', 'Ớt', 'Ot', '0', 1, 1),
('MAT12', 'Hành', 'Hanh', '0', 1, 1),
('MAT13', 'Tỏi', 'Toi', '0', 1, 1),
('MAT14', 'Cà rốt', 'Ca rot', '0', 1, 1),
('MAT15', 'Khoai tây', 'Khoai tay', '0', 1, 1),
('MAT16', 'Bắp cải', 'Bap cai', '0', 1, 1),
('MAT17', 'Cải thìa', 'Cai thia', '0', 1, 1),
('MAT18', 'Dưa chuột', 'Dua chuot', '0', 1, 1),
('MAT19', 'Táo', 'Tao', '0', 1, 1),
('MAT20', 'Chuối', 'Chuoi', '0', 1, 1);

-- Nhà cung cấp (20 dòng)
INSERT INTO "Vendor" ("Code", "Name", "NameNonUnicode", "Status", "CreatedBy", "UpdatedBy") VALUES
('VEND01', 'Công ty Thực phẩm ABC', 'Cong ty Thuc pham ABC', '0', 1, 1),
('VEND02', 'Siêu thị XYZ', 'Sieu thi XYZ', '0', 1, 1),
('VEND03', 'Cửa hàng Tiện lợi 123', 'Cua hang Tien loi 123', '0', 1, 1),
('VEND04', 'Công ty Nông sản Miền Tây', 'Cong ty Nong san Mien Tay', '0', 1, 1),
('VEND05', 'Công ty Xuất nhập khẩu ABC', 'Cong ty Xuat nhap khau ABC', '0', 1, 1),
('VEND06', 'Nhà phân phối XYZ', 'Nha phan phoi XYZ', '0', 1, 1),
('VEND07', 'Cửa hàng Rau sạch', 'Cua hang Rau sach', '0', 1, 1),
('VEND08', 'Công ty Thực phẩm X', 'Cong ty Thuc pham X', '0', 1, 1),
('VEND09', 'Siêu thị An Bình', 'Sieu thi An Binh', '0', 1, 1),
('VEND10', 'Cửa hàng Tiện lợi 456', 'Cua hang Tien loi 456', '0', 1, 1),
('VEND11', 'Nhà cung cấp thực phẩm Y', 'Nha cung cap thuc pham Y', '0', 1, 1),
('VEND12', 'Công ty thực phẩm Z', 'Cong ty thuc pham Z', '0', 1, 1),
('VEND13', 'Đại lý phân phối A', 'Dai ly phan phoi A', '0', 1, 1),
('VEND14', 'Nhà cung cấp B', 'Nha cung cap B', '0', 1, 1),
('VEND15', 'Công ty Nông sản Đông Nam', 'Cong ty Nong san Dong Nam', '0', 1, 1),
('VEND16', 'Siêu thị Hòa Bình', 'Sieu thi Hoa Binh', '0', 1, 1),
('VEND17', 'Cửa hàng thực phẩm C', 'Cua hang thuc pham C', '0', 1, 1),
('VEND18', 'Nhà phân phối D', 'Nha phan phoi D', '0', 1, 1),
('VEND19', 'Công ty Thực phẩm E', 'Cong ty Thuc pham E', '0', 1, 1),
('VEND20', 'Cửa hàng G', 'Cua hang G', '0', 1, 1);

-- Khách hàng (20 dòng)
INSERT INTO "Customer" ("Code", "Name", "NameNonUnicode", "Type", "Status", "CreatedBy", "UpdatedBy") VALUES
('CUS01', 'Nguyễn Văn A', 'Nguyen Van A', '1', '0', 1, 1),
('CUS02', 'Trần Thị B', 'Tran Thi B', '1', '0', 1, 1),
('CUS03', 'Công ty TNHH Thực phẩm An Toàn', 'Cong ty TNHH Thuc pham An Toan', '0', '0', 1, 1),
('CUS04', 'Lê Văn C', 'Le Van C', '1', '0', 1, 1),
('CUS05', 'Phạm Thị D', 'Pham Thi D', '1', '0', 1, 1),
('CUS06', 'Công ty XYZ', 'Cong ty XYZ', '0', '0', 1, 1),
('CUS07', 'Nguyễn Thị E', 'Nguyen Thi E', '1', '0', 1, 1),
('CUS08', 'Trần Văn F', 'Tran Van F', '1', '0', 1, 1),
('CUS09', 'Công ty GHI', 'Cong ty GHI', '0', '0', 1, 1),
('CUS10', 'Lê Thị H', 'Le Thi H', '1', '0', 1, 1),
('CUS11', 'Nguyễn Văn I', 'Nguyen Van I', '1', '0', 1, 1),
('CUS12', 'Trần Thị J', 'Tran Thi J', '1', '0', 1, 1),
('CUS13', 'Công ty KLM', 'Cong ty KLM', '0', '0', 1, 1),
('CUS14', 'Lê Văn M', 'Le Van M', '1', '0', 1, 1),
('CUS15', 'Phạm Thị N', 'Pham Thi N', '1', '0', 1, 1),
('CUS16', 'Công ty NOP', 'Cong ty NOP', '0', '0', 1, 1),
('CUS17', 'Nguyễn Thị O', 'Nguyen Thi O', '1', '0', 1, 1),
('CUS18', 'Trần Văn P', 'Tran Van P', '1', '0', 1, 1),
('CUS19', 'Công ty QRS', 'Cong ty QRS', '0', '0', 1, 1),
('CUS20', 'Lê Thị T', 'Le Thi T', '1', '0', 1, 1);

-- Sản phẩm (20 dòng)
INSERT INTO "Product" (
    "Code", "Name", "NameNonUnicode", "Unit_ID", "Category_ID", "ProcessingType_ID",
    "TaxRate", "Material_ID", "CompanyTaxRate", "ConsumerTaxRate",
    "SpecialProductTaxRate_ID", "Status", "CreatedBy", "UpdatedBy"
) VALUES
('PROD01', 'Thịt heo đông lạnh', 'Thit heo dong lanh', 1, 1, 1, 8.00, 1, 5.00, 7.00, 1, '0', 1, 1),
('PROD02', 'Cá hồi đóng hộp', 'Ca hoi dong hop', 2, 2, 2, 12.00, 2, 6.00, 9.00, 2, '0', 1, 1),
('PROD03', 'Gạo sấy khô', 'Gao say kho', 1, 1, 3, 5.00, 3, 3.00, 4.00, 3, '0', 1, 1),
('PROD04', 'Muối tươi', 'Muoi tuoi', 4, 4, 4, 3.00, 4, 2.00, 2.50, 4, '0', 1, 1),
('PROD05', 'Đường kính', 'Duong kinh', 5, 1, 5, 4.00, 5, 3.00, 3.50, 5, '0', 1, 1),
('PROD06', 'Bơ nhạt', 'Bo nhat', 6, 6, 6, 6.00, 6, 4.00, 5.00, 6, '0', 1, 1),
('PROD07', 'Sữa tươi', 'Sua tuoi', 7, 10, 7, 10.00, 7, 6.00, 8.00, 7, '0', 1, 1),
('PROD08', 'Bột mì đa dụng', 'Bot mi da dung', 8, 6, 8, 7.00, 8, 5.00, 6.00, 8, '0', 1, 1),
('PROD09', 'Dầu ăn thực vật', 'Dau an thuc vat', 9, 1, 9, 9.00, 9, 7.00, 8.50, 9, '0', 1, 1),
('PROD10', 'Tiêu đen', 'Tieu den', 10, 4, 10, 3.00, 10, 2.50, 3.50, 10, '0', 1, 1),
('PROD11', 'Ớt bột', 'Ot bot', 11, 4, 11, 4.00, 11, 3.50, 4.00, 11, '0', 1, 1),
('PROD12', 'Hành tây', 'Hanh tay', 12, 7, 12, 2.00, 12, 2.00, 2.50, 12, '0', 1, 1),
('PROD13', 'Tỏi khô', 'Toi kho', 13, 4, 13, 3.00, 13, 2.50, 3.00, 13, '0', 1, 1),
('PROD14', 'Cà rốt tươi', 'Ca rot tuoi', 14, 7, 14, 2.50, 14, 2.00, 2.50, 14, '0', 1, 1),
('PROD15', 'Khoai tây', 'Khoai tay', 15, 7, 15, 3.00, 15, 2.50, 3.00, 15, '0', 1, 1),
('PROD16', 'Bắp cải', 'Bap cai', 16, 7, 16, 2.00, 16, 1.50, 2.00, 16, '0', 1, 1),
('PROD17', 'Cải thìa', 'Cai thia', 17, 7, 17, 2.00, 17, 1.50, 2.00, 17, '0', 1, 1),
('PROD18', 'Dưa chuột', 'Dua chuot', 18, 7, 18, 2.00, 18, 1.50, 2.00, 18, '0', 1, 1),
('PROD19', 'Táo đỏ', 'Tao do', 19, 19, 19, 4.00, 19, 3.00, 3.50, 19, '0', 1, 1),
('PROD20', 'Chuối tiêu', 'Chuoi tieu', 20, 20, 20, 3.00, 20, 2.50, 3.00, 20, '0', 1, 1);

-- Bảng liên kết Sản phẩm - Nhà cung cấp (20 dòng)
INSERT INTO "Product_Vendor" ("Vendor_ID", "Product_ID", "Price", "UnitPrice", "Priority") VALUES
(1, 1, 150000, 150000, 1),
(2, 2, 120000, 120000, 1),
(3, 3, 100000, 100000, 1),
(4, 4, 90000, 90000, 1),
(5, 5, 110000, 110000, 1),
(6, 6, 130000, 130000, 1),
(7, 7, 140000, 140000, 1),
(8, 8, 160000, 160000, 1),
(9, 9, 170000, 170000, 1),
(10, 10, 90000, 90000, 1),
(11, 11, 85000, 85000, 1),
(12, 12, 95000, 95000, 1),
(13, 13, 105000, 105000, 1),
(14, 14, 97000, 97000, 1),
(15, 15, 88000, 88000, 1),
(16, 16, 93000, 93000, 1),
(17, 17, 102000, 102000, 1),
(18, 18, 94000, 94000, 1),
(19, 19, 99000, 99000, 1),
(20, 20, 101000, 101000, 1);

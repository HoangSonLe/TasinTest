-- Xóa dữ liệu theo thứ tự tránh lỗi khóa ngoại
DELETE FROM "Purchase_Order_Item";
DELETE FROM "Purchase_Order";

-- Reset sequence cho bảng Purchase_Order và Purchase_Order_Item
ALTER SEQUENCE "Purchase_Order_ID_seq" RESTART WITH 1;
ALTER SEQUENCE "Purchase_Order_Item_ID_seq" RESTART WITH 1;

-- Thêm dữ liệu mẫu cho bảng Purchase_Order
-- Status values: New, Confirmed, Executed, Cancel (Pending removed)
INSERT INTO "Purchase_Order" ("Customer_ID", "TotalPrice", "TotalPriceNoTax", "Code", "Status", "CreatedBy", "UpdatedBy")
VALUES
(1, 1500000.00, 1400000.00, 'PO001', 'Confirmed', 1, 1),  -- Confirmed
(2, 1200000.00, 1100000.00, 'PO002', 'Confirmed', 1, 1),  -- Confirmed
(3, 800000.00, 750000.00, 'PO003', 'New', 1, 1),         -- New
(4, 900000.00, 850000.00, 'PO004', 'Cancel', 1, 1),      -- Cancel
(1, 1300000.00, 1200000.00, 'PO005', 'Confirmed', 1, 1),  -- Confirmed
(2, 1100000.00, 1050000.00, 'PO006', 'Confirmed', 1, 1),  -- Confirmed
(3, 700000.00, 670000.00, 'PO007', 'New', 1, 1),         -- New
(4, 950000.00, 900000.00, 'PO008', 'Cancel', 1, 1),      -- Cancel
(1, 1600000.00, 1550000.00, 'PO009', 'Confirmed', 1, 1),  -- Confirmed
(2, 1250000.00, 1200000.00, 'PO010', 'Confirmed', 1, 1),  -- Confirmed
(3, 850000.00, 820000.00, 'PO011', 'New', 1, 1),         -- New
(4, 970000.00, 930000.00, 'PO012', 'Cancel', 1, 1),      -- Cancel
(1, 1400000.00, 1350000.00, 'PO013', 'Confirmed', 1, 1),  -- Confirmed
(2, 1150000.00, 1120000.00, 'PO014', 'Confirmed', 1, 1),  -- Confirmed
(3, 780000.00, 750000.00, 'PO015', 'New', 1, 1),         -- New
(4, 920000.00, 890000.00, 'PO016', 'Cancel', 1, 1),      -- Cancel
(1, 1350000.00, 1300000.00, 'PO017', 'Confirmed', 1, 1),  -- Confirmed
(2, 1180000.00, 1150000.00, 'PO018', 'Confirmed', 1, 1),  -- Confirmed
(3, 810000.00, 780000.00, 'PO019', 'New', 1, 1),         -- New
(4, 940000.00, 910000.00, 'PO020', 'Cancel', 1, 1);      -- Cancel

-- Thêm dữ liệu mẫu cho bảng Purchase_Order_Item
INSERT INTO "Purchase_Order_Item" ("PO_ID", "Product_ID", "Quantity", "Unit_ID", "Price", "TaxRate", "ProcessingType_ID", "LossRate", "ProcessingFee", "Note", "ProfitMargin")
VALUES
(1, 1, 10, 1, 150000, 8.00, 1, 0.5, 1000, 'Giao hàng nhanh', 5.00),
(1, 2, 5, 2, 120000, 12.00, 2, 0.3, 800, 'Hàng đóng hộp', 6.00),
(2, 3, 20, 1, 100000, 5.00, 3, 0.2, 500, 'Sấy khô', 4.00),
(2, 4, 15, 4, 90000, 3.00, 4, 0.1, 600, 'Tươi sống', 3.50),
(3, 1, 12, 1, 150000, 8.00, 1, 0.5, 1100, 'Giao nhanh', 5.00),
(3, 2, 7, 2, 120000, 12.00, 2, 0.3, 900, 'Hàng đóng hộp', 6.00),
(4, 3, 18, 1, 100000, 5.00, 3, 0.2, 550, 'Sấy khô', 4.00),
(4, 4, 14, 4, 90000, 3.00, 4, 0.1, 650, 'Tươi sống', 3.50),
(5, 1, 11, 1, 150000, 8.00, 1, 0.5, 1050, 'Giao hàng nhanh', 5.00),
(5, 2, 6, 2, 120000, 12.00, 2, 0.3, 820, 'Hàng đóng hộp', 6.00),
(6, 3, 19, 1, 100000, 5.00, 3, 0.2, 520, 'Sấy khô', 4.00),
(6, 4, 16, 4, 90000, 3.00, 4, 0.1, 610, 'Tươi sống', 3.50),
(7, 1, 13, 1, 150000, 8.00, 1, 0.5, 1150, 'Giao nhanh', 5.00),
(7, 2, 8, 2, 120000, 12.00, 2, 0.3, 910, 'Hàng đóng hộp', 6.00),
(8, 3, 17, 1, 100000, 5.00, 3, 0.2, 560, 'Sấy khô', 4.00),
(8, 4, 13, 4, 90000, 3.00, 4, 0.1, 630, 'Tươi sống', 3.50),
(9, 1, 14, 1, 150000, 8.00, 1, 0.5, 1080, 'Giao hàng nhanh', 5.00),
(9, 2, 9, 2, 120000, 12.00, 2, 0.3, 840, 'Hàng đóng hộp', 6.00),
(10, 3, 21, 1, 100000, 5.00, 3, 0.2, 530, 'Sấy khô', 4.00),
(10, 4, 15, 4, 90000, 3.00, 4, 0.1, 620, 'Tươi sống', 3.50);

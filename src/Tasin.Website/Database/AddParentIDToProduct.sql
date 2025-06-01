-- Thêm cột ParentID cho bảng Product (PostgreSQL)
ALTER TABLE "Product" ADD COLUMN "ParentID" INTEGER NULL;

-- Thêm foreign key constraint
ALTER TABLE "Product" ADD CONSTRAINT "FK_Product_Parent" FOREIGN KEY ("ParentID") REFERENCES "Product"("ID");

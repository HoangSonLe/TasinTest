CREATE TABLE "Vendor" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Address" TEXT,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100)
)

CREATE TABLE "Unit" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Name_EN" VARCHAR(255),
    "Description" TEXT,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE
)

CREATE TABLE "Category" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Name_EN" VARCHAR(255),
    "Parent_ID" INTEGER,
    "Description" TEXT,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    CONSTRAINT "fk_parent" FOREIGN KEY ("Parent_ID") REFERENCES "Category"("ID") ON DELETE SET NULL
)

CREATE TABLE "ProcessingType" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Name_EN" VARCHAR(255),
    "Description" TEXT,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE
)

CREATE TABLE "Customer" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Type" VARCHAR(50),
    "PhoneContact" VARCHAR(50),
    "Email" VARCHAR(255),
    "TaxCode" VARCHAR(100),
    "Address" TEXT,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    "IsActived" BOOLEAN default true
)

CREATE TABLE "SpecialProductTaxRate" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Name_EN" VARCHAR(255),
    "Description" TEXT,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE
)

CREATE TABLE "TaxRateConfig" (
    "ID" SERIAL PRIMARY KEY,
    "CompanyTaxRate" NUMERIC(5, 2) NOT NULL,
    "ConsumerTaxRate" NUMERIC(5, 2) NOT NULL,
    "SpecialProductTaxRate_ID" INTEGER,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "fk_special_product_taxrate" FOREIGN KEY ("SpecialProductTaxRate_ID") REFERENCES "SpecialProductTaxRate"("ID") ON DELETE SET NULL
)

CREATE TABLE "Material" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Name_EN" VARCHAR(255),
    "Parent_ID" INTEGER,
    "Description" TEXT,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    CONSTRAINT "fk_material_parent" FOREIGN KEY ("Parent_ID") REFERENCES "Material"("ID") ON DELETE SET NULL
)

CREATE TABLE "Product" (
    "ID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255),
    "Name_EN" VARCHAR(255),
    "Unit_ID" INTEGER,
    "Category_ID" INTEGER,
    "ProcessingType_ID" INTEGER,
    "TaxRate" NUMERIC(5, 2),
    "TaxRateConfig_ID" INTEGER,
    "LossRate" NUMERIC(5, 2),
    "Material_ID" INTEGER,
    "ProfitMargin" NUMERIC(5, 2),
    "Note" TEXT,
    "IsDiscontinued" BOOLEAN DEFAULT FALSE,
    "ProcessingFee" NUMERIC(18, 2),
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "fk_unit" FOREIGN KEY ("Unit_ID") REFERENCES "Unit"("ID"),
    CONSTRAINT "fk_category" FOREIGN KEY ("Category_ID") REFERENCES "Category"("ID"),
    CONSTRAINT "fk_processingtype" FOREIGN KEY ("ProcessingType_ID") REFERENCES "ProcessingType"("ID"),
    CONSTRAINT "fk_taxrateconfig" FOREIGN KEY ("TaxRateConfig_ID") REFERENCES "TaxRateConfig"("ID"),
    CONSTRAINT "fk_material" FOREIGN KEY ("Material_ID") REFERENCES "Material"("ID")
)

CREATE TABLE "Product_Vendor" (
    "Vendor_ID" INTEGER NOT NULL,
    "Product_ID" INTEGER NOT NULL,
    "Price" NUMERIC(18, 2),
    "UnitPrice" NUMERIC(18, 2),
    "Priority" INTEGER,
    "Description" TEXT,
    PRIMARY KEY ("Vendor_ID", "Product_ID"),
    FOREIGN KEY ("Vendor_ID") REFERENCES "Vendor"("ID"),
    FOREIGN KEY ("Product_ID") REFERENCES "Product"("ID")
)

CREATE TABLE "Purchase_Order" (
    "ID" SERIAL PRIMARY KEY,
    "Customer_ID" INTEGER NOT NULL,
    "TotalPrice" NUMERIC(18, 2) NOT NULL,
    "TotalPriceNoTax" NUMERIC(18, 2) NOT NULL,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "fk_customer" FOREIGN KEY ("Customer_ID") REFERENCES "Customer"("ID")
)

CREATE TABLE "Purchase_Order_Item" (
    "PO_ID" INTEGER NOT NULL,
    "ID" SERIAL NOT NULL,
    "Product_ID" INTEGER NOT NULL,
    "Quantity" NUMERIC(18, 2) NOT NULL,
    "Unit_ID" INTEGER,
    "Price" NUMERIC(18, 2),
    "TaxRate" NUMERIC(5, 2),
    "ProcessingType_ID" INTEGER,
    "LossRate" NUMERIC(5, 2),
    "ProcessingFee" NUMERIC(18, 2),
    "Note" TEXT,
    "ProfitMargin" NUMERIC(5, 2),
    PRIMARY KEY ("PO_ID", "ID"),
    CONSTRAINT "fk_po" FOREIGN KEY ("PO_ID") REFERENCES "Purchase_Order"("ID") ON DELETE CASCADE,
    CONSTRAINT "fk_product" FOREIGN KEY ("Product_ID") REFERENCES "Product"("ID"),
    CONSTRAINT "fk_unit" FOREIGN KEY ("Unit_ID") REFERENCES "Unit"("ID"),
    CONSTRAINT "fk_processingtype" FOREIGN KEY ("ProcessingType_ID") REFERENCES "ProcessingType"("ID")
)

CREATE TABLE "Purchase_Agreement" (
    "ID" SERIAL PRIMARY KEY,
    "Vendor_ID" INTEGER NOT NULL,
    "Note" TEXT,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "TotalPrice" NUMERIC(18, 2) NOT NULL,
    "Status" VARCHAR(50),
    "IsActived" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "fk_vendor" FOREIGN KEY ("Vendor_ID") REFERENCES "Vendor"("ID")
)

CREATE TABLE "Purchase_Agreement_Item" (
    "PA_ID" INTEGER NOT NULL,
    "Product_ID" INTEGER NOT NULL,
    "Quantity" NUMERIC(18, 2) NOT NULL,
    "Unit_ID" INTEGER,
    "Price" NUMERIC(18, 2),
    "PO_Item_ID_List" TEXT,
    PRIMARY KEY ("PA_ID", "Product_ID"),
    CONSTRAINT "fk_pa" FOREIGN KEY ("PA_ID") REFERENCES "Purchase_Agreement"("ID") ON DELETE CASCADE,
    CONSTRAINT "fk_product" FOREIGN KEY ("Product_ID") REFERENCES "Product"("ID"),
    CONSTRAINT "fk_unit" FOREIGN KEY ("Unit_ID") REFERENCES "Unit"("ID")
)

CREATE TABLE "CodeVersion" (
    "ID" SERIAL PRIMARY KEY,
    "Prefix" VARCHAR(50) NOT NULL,
    "VersionIndex" INTEGER NOT NULL,
    "Type" VARCHAR(50),
    "Note" TEXT
)


CREATE TABLE "Role" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Level" INTEGER NOT NULL,
    "EnumActionList" VARCHAR(1000) NOT NULL DEFAULT ''  -- lưu danh sách dạng chuỗi
)

CREATE TABLE "User" (
    "Id" SERIAL PRIMARY KEY,
    "UserName" VARCHAR(255) NOT NULL,
    "Password" VARCHAR(255) NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "NameNonUnicode" VARCHAR(255) NOT NULL,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Email" VARCHAR(255),
    "Address" TEXT,
    "Phone" VARCHAR(50) NOT NULL DEFAULT '',
    "RoleIdList" TEXT NOT NULL DEFAULT '', -- lưu danh sách role IDs dưới dạng chuỗi (ví dụ: '1,2,3')
    "TypeAccount" INTEGER NOT NULL,
    "CreatedDate" TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP WITHOUT TIME ZONE,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    "IsActived" BOOLEAN default true,

)

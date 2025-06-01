-- Migration script to convert ProcessingType_ID to ProcessingType enum
-- This script converts the ProcessingType_ID foreign key to ProcessingType enum field

-- Step 1: Add new ProcessingType column as varchar (enum will be stored as string)
ALTER TABLE "Product" ADD COLUMN "ProcessingType" VARCHAR(50) NOT NULL DEFAULT 'Material';
ALTER TABLE "Purchase_Order_Item" ADD COLUMN "ProcessingType" VARCHAR(50) NOT NULL DEFAULT 'Material';

-- Step 2: Update ProcessingType based on existing ProcessingType_ID
-- Assuming ProcessingType table has these mappings:
-- ID 1 = "Nguyên liệu" -> EProcessingType.Material = "Material"
-- ID 2 = "Sơ chế" -> EProcessingType.SemiProcessed = "SemiProcessed"
-- ID 3 = "Thành phẩm" -> EProcessingType.FinishedProduct = "FinishedProduct"

-- Update based on ProcessingType names or IDs
UPDATE "Product"
SET "ProcessingType" = CASE
    WHEN "ProcessingType_ID" IS NULL THEN 'Material' -- Default to Material
    WHEN "ProcessingType_ID" IN (
        SELECT pt."ID" FROM "ProcessingType" pt
        WHERE pt."Name" LIKE '%Nguyên liệu%' OR pt."Name" LIKE '%Material%'
    ) THEN 'Material' -- Material
    WHEN "ProcessingType_ID" IN (
        SELECT pt."ID" FROM "ProcessingType" pt
        WHERE pt."Name" LIKE '%Sơ chế%' OR pt."Name" LIKE '%Semi%'
    ) THEN 'SemiProcessed' -- SemiProcessed
    WHEN "ProcessingType_ID" IN (
        SELECT pt."ID" FROM "ProcessingType" pt
        WHERE pt."Name" LIKE '%Thành phẩm%' OR pt."Name" LIKE '%Finished%'
    ) THEN 'FinishedProduct' -- FinishedProduct
    ELSE 'Material' -- Default to Material if no match
END;

-- Step 3: Update Purchase_Order_Item ProcessingType based on existing ProcessingType_ID
UPDATE "Purchase_Order_Item"
SET "ProcessingType" = CASE
    WHEN "ProcessingType_ID" IS NULL THEN 'Material' -- Default to Material
    WHEN "ProcessingType_ID" IN (
        SELECT pt."ID" FROM "ProcessingType" pt
        WHERE pt."Name" LIKE '%Nguyên liệu%' OR pt."Name" LIKE '%Material%'
    ) THEN 'Material' -- Material
    WHEN "ProcessingType_ID" IN (
        SELECT pt."ID" FROM "ProcessingType" pt
        WHERE pt."Name" LIKE '%Sơ chế%' OR pt."Name" LIKE '%Semi%'
    ) THEN 'SemiProcessed' -- SemiProcessed
    WHEN "ProcessingType_ID" IN (
        SELECT pt."ID" FROM "ProcessingType" pt
        WHERE pt."Name" LIKE '%Thành phẩm%' OR pt."Name" LIKE '%Finished%'
    ) THEN 'FinishedProduct' -- FinishedProduct
    ELSE 'Material' -- Default to Material if no match
END;

-- Step 4: Drop foreign key constraints first
ALTER TABLE "Purchase_Order_Item" DROP CONSTRAINT IF EXISTS "fk_processingtype";

-- Step 5: Drop the old ProcessingType_ID columns
ALTER TABLE "Product" DROP COLUMN "ProcessingType_ID";
ALTER TABLE "Purchase_Order_Item" DROP COLUMN "ProcessingType_ID";

-- Step 6: Drop ProcessingType table (optional - only if not used elsewhere)
-- Uncomment the following lines if you want to completely remove ProcessingType table
-- DROP TABLE IF EXISTS "ProcessingType";

-- Verification queries to check the migration
-- SELECT "ID", "Name", "ProcessingType", "IsMaterial" FROM "Product" LIMIT 10;
-- SELECT "ID", "PO_ID", "Product_ID", "ProcessingType" FROM "Purchase_Order_Item" LIMIT 10;

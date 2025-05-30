-- Migration script to replace Material_ID with IsMaterial in Product table
-- Date: $(date)
-- Description: Remove Material_ID foreign key and add IsMaterial boolean field

BEGIN;

-- Step 1: Add IsMaterial column to Product table
ALTER TABLE "Product" ADD COLUMN "IsMaterial" BOOLEAN DEFAULT FALSE;

-- Step 2: Update IsMaterial based on existing Material_ID values
-- Set IsMaterial = true where Material_ID is not null
UPDATE "Product" 
SET "IsMaterial" = TRUE 
WHERE "Material_ID" IS NOT NULL;

-- Set IsMaterial = false where Material_ID is null (this is already the default, but for clarity)
UPDATE "Product" 
SET "IsMaterial" = FALSE 
WHERE "Material_ID" IS NULL;

-- Step 3: Drop the foreign key constraint for Material_ID (if exists)
-- Note: Check constraint name in your database, it might be different
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints 
        WHERE constraint_name = 'fk_product_material' 
        AND table_name = 'Product'
    ) THEN
        ALTER TABLE "Product" DROP CONSTRAINT "fk_product_material";
    END IF;
END $$;

-- Step 4: Drop Material_ID column
ALTER TABLE "Product" DROP COLUMN IF EXISTS "Material_ID";

-- Step 5: Set IsMaterial as NOT NULL with default value
ALTER TABLE "Product" ALTER COLUMN "IsMaterial" SET NOT NULL;
ALTER TABLE "Product" ALTER COLUMN "IsMaterial" SET DEFAULT FALSE;

COMMIT;

-- Verification queries (run these to verify the migration)
-- SELECT COUNT(*) as total_products FROM "Product";
-- SELECT COUNT(*) as products_with_material FROM "Product" WHERE "IsMaterial" = TRUE;
-- SELECT COUNT(*) as products_without_material FROM "Product" WHERE "IsMaterial" = FALSE;
-- SELECT "IsMaterial", COUNT(*) FROM "Product" GROUP BY "IsMaterial";

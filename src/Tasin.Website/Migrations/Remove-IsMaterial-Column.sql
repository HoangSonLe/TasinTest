-- Migration script to remove IsMaterial column from Product table
-- Date: $(date)
-- Description: Remove IsMaterial boolean field since ProcessingType enum now handles this logic

BEGIN;

-- Step 1: Verify that all products have ProcessingType set
-- This is a safety check to ensure data integrity
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM "Product" 
        WHERE "ProcessingType" IS NULL OR "ProcessingType" = ''
    ) THEN
        RAISE EXCEPTION 'Cannot remove IsMaterial column: Some products have null or empty ProcessingType';
    END IF;
END $$;

-- Step 2: Drop IsMaterial column
ALTER TABLE "Product" DROP COLUMN IF EXISTS "IsMaterial";

COMMIT;

-- Verification queries (run these to verify the migration)
-- SELECT COUNT(*) as total_products FROM "Product";
-- SELECT "ProcessingType", COUNT(*) FROM "Product" GROUP BY "ProcessingType";
-- SELECT * FROM "Product" WHERE "ProcessingType" = 'Material' LIMIT 5;

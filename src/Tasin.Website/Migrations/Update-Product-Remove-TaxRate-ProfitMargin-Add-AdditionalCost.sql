-- Migration script to remove TaxRate, ProfitMargin and add AdditionalCost to Product table
-- Date: $(date)
-- Description: Remove TaxRate and ProfitMargin fields, add AdditionalCost field

BEGIN;

-- Step 1: Add AdditionalCost column
ALTER TABLE "Product" 
ADD COLUMN IF NOT EXISTS "AdditionalCost" NUMERIC(18, 2);

-- Step 2: Drop TaxRate column
ALTER TABLE "Product" 
DROP COLUMN IF EXISTS "TaxRate";

-- Step 3: Drop ProfitMargin column  
ALTER TABLE "Product" 
DROP COLUMN IF EXISTS "ProfitMargin";

COMMIT;

-- Verification queries (run these to verify the migration)
-- SELECT column_name, data_type, is_nullable 
-- FROM information_schema.columns 
-- WHERE table_name = 'Product' 
-- AND column_name IN ('AdditionalCost', 'TaxRate', 'ProfitMargin')
-- ORDER BY column_name;

-- SELECT COUNT(*) as total_products FROM "Product";
-- SELECT * FROM "Product" LIMIT 5;

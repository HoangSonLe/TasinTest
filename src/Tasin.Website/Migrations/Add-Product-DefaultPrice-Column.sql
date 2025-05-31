-- Migration: Add DefaultPrice column to Product table
-- Date: 2025-01-27
-- Description: Add DefaultPrice column to separate default selling price from profit margin percentage

-- Step 1: Add DefaultPrice column to Product table
ALTER TABLE "Product" 
ADD COLUMN "DefaultPrice" DECIMAL(18,2) NULL;

-- Step 2: Add comment to clarify the purpose
COMMENT ON COLUMN "Product"."DefaultPrice" IS 'Default selling price for the product (VND)';
COMMENT ON COLUMN "Product"."ProfitMargin" IS 'Profit margin percentage (%)';

-- Step 3: Create index for better query performance (optional)
CREATE INDEX IF NOT EXISTS "IX_Product_DefaultPrice" ON "Product" ("DefaultPrice");

-- Step 4: Update existing data (optional - set DefaultPrice = ProfitMargin for existing records)
-- Uncomment the line below if you want to migrate existing data
-- UPDATE "Product" SET "DefaultPrice" = "ProfitMargin" WHERE "ProfitMargin" IS NOT NULL;

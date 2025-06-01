-- =============================================
-- Script: Update Purchase_Order_Item table
-- Description: Remove ProcessingType and ProfitMargin columns, Add AdditionalCost column
-- Date: 2024-12-19
-- =============================================

-- Step 1: Add AdditionalCost column
ALTER TABLE "Purchase_Order_Item" 
ADD COLUMN "AdditionalCost" NUMERIC(18, 2);

-- Step 2: Drop ProcessingType column (if exists)
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'Purchase_Order_Item' 
               AND column_name = 'ProcessingType') THEN
        ALTER TABLE "Purchase_Order_Item" DROP COLUMN "ProcessingType";
    END IF;
END $$;

-- Step 3: Drop ProcessingType_ID column (if exists)
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'Purchase_Order_Item' 
               AND column_name = 'ProcessingType_ID') THEN
        ALTER TABLE "Purchase_Order_Item" DROP COLUMN "ProcessingType_ID";
    END IF;
END $$;

-- Step 4: Drop ProfitMargin column (if exists)
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'Purchase_Order_Item' 
               AND column_name = 'ProfitMargin') THEN
        ALTER TABLE "Purchase_Order_Item" DROP COLUMN "ProfitMargin";
    END IF;
END $$;

-- Step 5: Drop foreign key constraint for ProcessingType (if exists)
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
               WHERE table_name = 'Purchase_Order_Item' 
               AND constraint_name = 'fk_processingtype') THEN
        ALTER TABLE "Purchase_Order_Item" DROP CONSTRAINT "fk_processingtype";
    END IF;
END $$;

-- Verification queries
SELECT 'Verification: Purchase_Order_Item table structure after changes:' as message;

SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'Purchase_Order_Item' 
ORDER BY ordinal_position;

-- Check if AdditionalCost column was added successfully
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns 
                     WHERE table_name = 'Purchase_Order_Item' 
                     AND column_name = 'AdditionalCost') 
        THEN 'SUCCESS: AdditionalCost column added'
        ELSE 'ERROR: AdditionalCost column not found'
    END as additionalcost_status;

-- Check if ProcessingType columns were removed successfully
SELECT 
    CASE 
        WHEN NOT EXISTS (SELECT 1 FROM information_schema.columns 
                         WHERE table_name = 'Purchase_Order_Item' 
                         AND column_name IN ('ProcessingType', 'ProcessingType_ID')) 
        THEN 'SUCCESS: ProcessingType columns removed'
        ELSE 'ERROR: ProcessingType columns still exist'
    END as processingtype_status;

-- Check if ProfitMargin column was removed successfully
SELECT 
    CASE 
        WHEN NOT EXISTS (SELECT 1 FROM information_schema.columns 
                         WHERE table_name = 'Purchase_Order_Item' 
                         AND column_name = 'ProfitMargin') 
        THEN 'SUCCESS: ProfitMargin column removed'
        ELSE 'ERROR: ProfitMargin column still exists'
    END as profitmargin_status;

-- Sample data check (if table has data)
SELECT 'Sample data after migration (first 5 rows):' as message;

SELECT "PO_ID", "ID", "Product_ID", "Quantity", "Price", "TaxRate", "LossRate", "AdditionalCost", "ProcessingFee", "Note"
FROM "Purchase_Order_Item" 
LIMIT 5;

-- Count total records
SELECT COUNT(*) as total_purchase_order_items FROM "Purchase_Order_Item";

COMMIT;

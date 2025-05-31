-- Migration: Update Purchase_Order_Item Primary Key Structure
-- Date: 2025-01-27
-- Description: Change from composite primary key (PO_ID, ID) to single primary key (ID)
--              and add unique constraint for (PO_ID, Product_ID)

-- Step 1: Drop the existing primary key constraint
ALTER TABLE "Purchase_Order_Item" DROP CONSTRAINT "Purchase_Order_Item_pkey";

-- Step 2: Create new primary key on ID only
ALTER TABLE "Purchase_Order_Item" ADD CONSTRAINT "Purchase_Order_Item_pkey" PRIMARY KEY ("ID");

-- Step 3: Add unique constraint for PO_ID and Product_ID combination
-- This ensures we don't have duplicate products in the same purchase order
ALTER TABLE "Purchase_Order_Item" 
ADD CONSTRAINT "IX_Purchase_Order_Item_PO_Product" 
UNIQUE ("PO_ID", "Product_ID");

-- Step 4: Add index for better query performance
CREATE INDEX IF NOT EXISTS "IX_Purchase_Order_Item_PO_ID" ON "Purchase_Order_Item" ("PO_ID");
CREATE INDEX IF NOT EXISTS "IX_Purchase_Order_Item_Product_ID" ON "Purchase_Order_Item" ("Product_ID");

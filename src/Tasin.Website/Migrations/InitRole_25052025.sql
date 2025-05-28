
INSERT INTO public."Role"
("Id", "Name", "NameNonUnicode", "Description", "Level", "EnumActionList") 
VALUES(1, 'Super Admin', 'Super Admin', 'Super Admin', 1, '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47');
INSERT INTO public."Role"
("Id", "Name", "NameNonUnicode", "Description", "Level", "EnumActionList") 
VALUES(2, 'Admin', 'Admin', 'Admin', 2, '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47');
INSERT INTO public."Role"
("Id", "Name", "NameNonUnicode", "Description", "Level", "EnumActionList") 
VALUES(3, 'User', 'User', 'Người dùng', 3, '36,37,38,39,40,41,42,43');


UPDATE public."User" SET "RoleIdList"='1' WHERE "UserName"='ADMIN';


ALTER TABLE public."Category" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Customer" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Material" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."ProcessingType" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Product" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Purchase_Agreement" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Purchase_Order" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."SpecialProductTaxRate" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Unit" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."User" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public."Vendor" ALTER COLUMN "UpdatedDate" SET DEFAULT CURRENT_TIMESTAMP;

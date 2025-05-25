INSERT INTO public."Role"
("Id", "Name", "NameNonUnicode", "Description", "Level", "EnumActionList")
VALUES(1, 'Admin', 'Admin', 'Admin', 1, '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47');
INSERT INTO public."Role"
("Id", "Name", "NameNonUnicode", "Description", "Level", "EnumActionList")
VALUES(2, 'Super Admin', 'Super Admin', 'Super Admin', 0, '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47');
INSERT INTO public."Role"
("Id", "Name", "NameNonUnicode", "Description", "Level", "EnumActionList")
VALUES(3, 'User', 'User', 'Người dùng', 2, '36,37,38,39,40,41,42,43');


UPDATE public."User" SET "RoleIdList"='1' WHERE "UserName"='ADMIN';
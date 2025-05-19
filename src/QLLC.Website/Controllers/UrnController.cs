using ClosedXML.Excel;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using System.Security.Claims;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class UrnController : BaseController<UrnController>
    {
        private readonly IUrnService _urnService;
        private readonly IConfigService _configService;
        public UrnController(
            IUserService userService,
            IUrnService urnService,
            IConfigService configService,
             ILogger<UrnController> logger
            ) : base(logger, userService)
        {
            _urnService = urnService;
            _configService = configService;
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_URN])]
        public async Task<IActionResult> Index()
        {

            //var ss = "tUw+189cRjNokHkAPP+6Kg==";
            //var sss = Utils.DecodePassword(ss, EEncodeType.SHA_256);
            ViewBag.GenderDatasource = EnumHelper.ToDropdownList<EGender>();
            ViewBag.UrnTypeDatasource = EnumHelper.ToDropdownList<EUrnType>();
            ViewBag.IsMobile = _isMobile;
            if (_isMobile)
            {
                var id = int.Parse(_currentUserId);
                var _user =await UserService.GetUserById(id);
                if (_user.IsSuccess)
                {
                    if(_user.Data.RoleIdList.Contains((int)ERoleType.User))
                        return View("Index_mobile");
                }
            }

            return View();
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_URN])]
        public async Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnList(UrnSearchModel searchModel)
        {
            return await _urnService.GetUrnList(searchModel);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_URN, (int)EActionRole.UPDATE_URN])]
        [HttpGet]
        public async Task<Acknowledgement<UrnViewModel>> GetUrnById(long urnId)
        {
            return await _urnService.GetUrnById(urnId);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_URN, (int)EActionRole.UPDATE_URN])]
        [HttpPost]
        public async Task<Acknowledgement> CreateOrUpdateUrn([FromBody]UrnViewModel postData)
        {
            return await _urnService.CreateOrUpdateUrn(postData);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_URN])]
        [HttpGet]
        public async Task<Acknowledgement> DeleteUrnById(long urnId)
        {
            return await _urnService.DeleteUrnById(urnId);

        }


        /// <summary>
        /// Lấy ds gần tới giỗ
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_URN])]
        public async Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnWorshipDayList(UrnSearchModel searchModel)
        {
            searchModel.IsFilterAnniversary = true;
            return await _urnService.GetUrnWorshipDayList(searchModel);
        }
        /// <summary>
        /// Lấy ds gần tới ký gửi
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_URN])]
        public async Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnConsignmentExpired(UrnSearchModel searchModel)
        {
            searchModel.IsFilterAnniversary = false;
            return await _urnService.GetUrnWorshipDayList(searchModel);
        }

        public class ExcelUserError
        {
            public int medicine_id { get; set; }
            public string medicine_code { get; set; }
            public string medicine_name { get; set; }
            public double medicine_cost { get; set; }
            public string description { get; set; }

            public string error_message { get; set; }
        }

        [HttpPost]
        public async Task<Acknowledgement<List<UserViewModel>>> ImportExcelFile(IFormFile formFile)
        {
            var response = new Acknowledgement<List<UserViewModel>>();
            try { 
                if (formFile != null)
                {
                    if (!FileHelper.ValidateFileExt(EFileType.Excel, Path.GetExtension(formFile.FileName)))
                    {
                        throw new Exception("Định dạng file không đúng!");
                    }
                    try
                    {
                        XLWorkbook workbook = new XLWorkbook(formFile.OpenReadStream());
                        var sheets = workbook.Worksheets.First();
                        var rows = sheets.Rows().ToList();

                        var dateTime = DateTime.Now;
                        var predicate = PredicateBuilder.New<User>(false);
                        var newMedicineItemList = new List<User>();
                        var errorList = new List<ExcelUserError>();

                        var listError = new List<UserViewModel>();

                        var TotalRow = rows.Count();
                        if (TotalRow < 7) throw new Exception("Tập tin không có dữ liệu!"); ;
                        var currentUserId = _currentUserId;

                        var count = 0;

                        for (var i = 7; i < TotalRow; ++i)//Start from row 1 : Skip header row in excel
                        {
                            //Covert excel to medicine Obj
                            var row = rows[i];
                            UserViewModel item = new UserViewModel();
                            if (count == 10) break;
                            if (string.IsNullOrEmpty(row.Cell(1).Value.ToString())) { 
                                count++;
                                continue;
                            }
                            else
                            {
                                count = 0;
                            }

                            item.Name = row.Cell(3).Value.ToString();
                            item.Phone = row.Cell(6).Value.ToString();
                            if (string.IsNullOrEmpty(item.Name))
                            {
                                continue;
                            }
                            item.UserName = Utils.RemoveSignAndLowerCaseVietnameseString(item.Name.ToUpper().Replace(" ","")).ToLower();
                            item.Password = "123456aA@";
                            item.RoleIdList = new List<int> { (short)ERoleType.User };
                            //double cost;
                            //double.TryParse(row.Cell(3).Value.ToString(), out cost);
                            //item.medicine_cost = cost;
                            //item.description = row.Cell(4).Value.ToString();
                            //item.created_at = dateTime;
                            //item.updated_at = dateTime;
                            //item.updated_by = int.Parse(currentUserId);
                            //item.created_by = int.Parse(currentUserId);
                            //item.state = (byte)EState.Active;
                            //item.medicine_cost_string = row.Cell(3).Value.ToString();
                            try
                            {

                                var rs =await UserService.CreateOrUpdateUser(item);
                                if(rs.IsSuccess==false)
                                    listError.Add(item);
                            }
                            catch
                            {
                            }

                        //    //Validate medicine's info (name, code, price)
                        //    var validateItem = ValidateInforMedicine(medicineItem);

                            //    if (!validateItem.IsSuccess)
                            //    {
                            //        errorList.Add(new ExcelMedicineError()
                            //        {
                            //            medicine_code = medicineItem.medicine_code,
                            //            medicine_name = medicineItem.medicine_name,
                            //            medicine_cost = (double)medicineItem.medicine_cost,
                            //            error_message = validateItem.Message
                            //        });
                            //    }
                            //    else
                            //    {
                            //        var newMedicine = new medicine()
                            //        {
                            //            medicine_id = medicineItem.medicine_id,
                            //            medicine_code = medicineItem.medicine_code,
                            //            medicine_name = medicineItem.medicine_name,
                            //            medicine_cost = (int)medicineItem.medicine_cost,
                            //            description = medicineItem.description,
                            //            created_at = medicineItem.created_at,
                            //            updated_at = medicineItem.updated_at,
                            //            updated_by = medicineItem.updated_by,
                            //            created_by = medicineItem.created_by,
                            //            state = medicineItem.state,
                            //        };
                            //        newMedicineItemList.Add(newMedicine);

                            //        //Predicate same medicine's infor (same code, same name, state: active)
                            //        predicate = predicate.Or(i => i.medicine_code == medicineItem.medicine_code
                            //                                                                  //&& i.medicine_name == medicineItem.medicine_name
                            //                                                                  && i.state == (byte)EState.Active
                            //                                                                  );
                            //    }

                            //}


                            ////Get list same medicine in DB (same code, same name, state: active)
                            //if (newMedicineItemList.Count > 0)
                            //{
                            //    var existMedicineDbItemList = await _urnService.Where(predicate).AsNoTracking().ToListAsync();

                            //    var joinList = from newItem in newMedicineItemList
                            //                   join db in existMedicineDbItemList on new { newItem.medicine_code } equals new { db.medicine_code } into g1
                            //                   from dbItem in g1.DefaultIfEmpty()
                            //                   select new { newItem, dbItem };

                            //    foreach (var item in joinList)
                            //    {
                            //        var newItem = item.newItem;
                            //        var dbItem = item.dbItem;

                            //        if (dbItem != null)
                            //        {
                            //            errorList.Add(new ExcelUserError()
                            //            {
                            //                medicine_code = newItem.medicine_code,
                            //                medicine_name = newItem.medicine_name,
                            //                medicine_cost = newItem.medicine_cost,
                            //                error_message = "Mã thuốc đã tồn tại"
                            //            });
                            //        }
                            //    }
                        }
                        ////If no error items => import database
                        if (listError.Count() > 0)
                        {
                            response.Data = listError ;
                            response.IsSuccess = false;
                            response.SuccessMessageList.Add("Không tìm thấy file!");
                            return response;
                        }
                        //else
                        //{
                        //    _context.medicine.AddRangeAsync(newMedicineItemList);
                        //    await _context.SaveChangesAsync();
                        //}

                        response.IsSuccess = true;
                        response.SuccessMessageList.Add("Thêm thành công!");

                        return response;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("ImportMedicineExcelFile " + e.Message);
                        response.IsSuccess = false;
                        return response;
                    }
                }
                response.IsSuccess = false;
                response.SuccessMessageList.Add( "Không tìm thấy file!");
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError("Urn ImportExcelFile " + ex.Message);
                response.IsSuccess = false;
                return response;
            }
}
    }
}

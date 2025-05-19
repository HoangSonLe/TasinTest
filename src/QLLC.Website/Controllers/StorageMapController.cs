using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using System.Net;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class StorageMapController : BaseController<StorageMapController>
    {
        private readonly IStorageMapService _storageMapService;
        private readonly IConfiguration _configuration;
        public StorageMapController(
            IUserService userService,
            IStorageMapService storageMapService,
             ILogger<StorageMapController> logger,
             IConfiguration configuration
            ) : base(logger, userService)
        {
            _storageMapService = storageMapService;
            _configuration = configuration;
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_STORAGE])]
        public IActionResult Index()
        {
            return View();
        }
        //[HttpGet]

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_STORAGE])]
        public async Task<Acknowledgement<JsonResultPaging<List<StorageMapViewModel>>>> GetStorageMapList(StorageMapSearchModel searchModel)
        {
            return await _storageMapService.GetStorageMapList(searchModel);
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_STORAGE , (int)EActionRole.UPDATE_STORAGE])]
        public async Task<Acknowledgement<StorageMapViewModel>> GetStorageMapById(long storageMapId)
        {
            return await _storageMapService.GetStorageMapById(storageMapId);
        }

        [HttpPost]

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_STORAGE,(int)EActionRole.UPDATE_STORAGE])]
        public async Task<Acknowledgement> CreateOrUpdateStorageMap([FromBody] StorageMapViewModel postData)
        {
            return await _storageMapService.CreateOrUpdateStorageMap(postData);
        }

        [HttpPost]

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_STORAGE])]
        public async Task<Acknowledgement> DeleteStorageMapById(long storageMapId)
        {
            return await _storageMapService.DeleteStorageMapById(storageMapId);

        }



        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            try
            {
                string now = DateTime.Now.ToString("dd-MM-yyyy");
                var fileName = DateTime.Now.Date.ToString("MM_dd_yyyy") + "_" + file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1);
                string path = "/Document/StorageMap/" + now + "/" + fileName;
                string checkDirectory = "Document/StorageMap/" + now + "/";

                Thread thread = new Thread(() =>
                {
                    try
                    {

                        if (file != null && file.Length > 0)
                        {

                            var userNameFTP = Utils.DecodePassword(_configuration.GetSection("FTP:UserName").Value, EEncodeType.SHA_256);
                            var passwordFTP = Utils.DecodePassword(_configuration.GetSection("FTP:Password").Value, EEncodeType.SHA_256);
                            var hostFTP = _configuration.GetSection("FTP:Host").Value + ":" + _configuration.GetSection("FTP:Port").Value;
                            string checkDirectoryTmp = hostFTP + "/";
                            foreach (var item in checkDirectory.Split("/"))
                            {
                                checkDirectoryTmp += item + "/";
                                if (!FtpFunction.GetDirectoryExits(checkDirectoryTmp, userNameFTP, passwordFTP))
                                {
                                    FtpFunction.CreateDirectory(checkDirectoryTmp, userNameFTP, passwordFTP);
                                }
                            }



                            byte[] fileBytes;

                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }

                            using (var client = new WebClient())
                            {
                                client.Credentials = new NetworkCredential(userNameFTP, passwordFTP);
                                var rs = client.UploadData(hostFTP + "//Document//StorageMap//" + now + "//" + fileName, fileBytes);

                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Logger.LogError("StorageMap Upload Thread " + e.Message);
                    }
                });


                return Json(new
                {
                    result = "success",
                    message = "Lưu thành công!",
                    path = path,
                });
            }
            catch (Exception ex)
            {
                Logger.LogError("StorageMap Upload " + ex.Message);
                return Json(new
                {
                    result = "error",
                    message = ex.Message
                });
            }

        }


    }
}

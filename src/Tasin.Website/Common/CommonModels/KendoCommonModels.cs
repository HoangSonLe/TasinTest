﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tasin.Website.Common.ConfigModel;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Common.CommonModels
{
    #region Paging
    public class PagingModel
    {
        public int PageNumber { get; set; } = DefaultConfig.DefaultPageNumber;
        public int PageSize { get; set; } = DefaultConfig.DefaultPageSize;
    }
    #endregion
    #region RESPONSE
    public class JsonResultPaging<T> : PagingModel where T : class
    {
        public T Data { get; set; }
        public int Total { get; set; }
    }
    //public class JsonKendoResult
    //{
    //    public string Message { get; set; } = string.Empty;
    //    public EKendoNotificationType TypeEnum { get; set; }
    //    public string Type => TypeEnum.GetEnumDescription();
    //    public JsonKendoResult()
    //    {
    //        Message = string.Empty;
    //        TypeEnum = EKendoNotificationType.ERROR;
    //    }
    //    public JsonKendoResult(string message, EKendoNotificationType typeEnum)
    //    {
    //        Message = message;
    //        TypeEnum = typeEnum;
    //    }
    //}
    //public class JsonKendoResult<T> : JsonKendoResult
    //{
    //    public T Data { get; set; }
    //    public JsonKendoResult()
    //    {
    //        Message = string.Empty;
    //        TypeEnum = EKendoNotificationType.INFO;
    //    }

    //    public JsonKendoResult(T data, string message, EKendoNotificationType typeEnum)
    //    {
    //        Data = data;
    //        TypeEnum = typeEnum;
    //        Message = message;
    //    }
    //}

    public class SearchPagingModel<T> : PagingModel
    {
        /// <summary>
        /// Search Model
        /// </summary>
        //public T SearchModel { get; set; }

        /// <summary>
        /// Search string to filter users by name, username, or phone
        /// </summary>
        [Display(Name = "Search")]
        public string? SearchString { get; set; }
    }
    #endregion
    #region RESPONSE SERVICE
    //public class ServiceResponseModel : JsonKendoResult
    //{
    //    public bool IsSuccess { get; set; }
    //    public ServiceResponseModel()
    //    {

    //    }
    //    public ServiceResponseModel(bool isSuccess)
    //    {
    //        IsSuccess = isSuccess;
    //    }
    //}

    //public class Acknowledgement<T> : ServiceResponseModel
    //{
    //    public T Data { get; set; }
    //}
    #endregion

    #region 
    public class KendoDropdownListModel<T>
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public dynamic Data { get; set; }
    }
    public class DropdownListModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
    #endregion
}

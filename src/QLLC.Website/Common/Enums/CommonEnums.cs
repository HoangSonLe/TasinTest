using Microsoft.OpenApi.Attributes;
using System.ComponentModel;

namespace Tasin.Website.Common.Enums
{
    /// Rule for naming : E + Noun  

    /// <summary>
    /// Encode password type
    /// </summary>
    public enum EEncodeType
    {
        SHA_256
    }
 
    /// <summary>
    /// Enum file type
    /// </summary>
    public enum EFileType
    {
        Excel,
        //Word,...
    }

    public enum ECategoryType
    {
        [Display("Loại khách hàng")]
        CustomerType,
        CommonStatusType,
        Unit,
        ProcessingType,
        Material,
        Vendor,
        Customer,
        Product
    }

    public enum ECommonStatus
    {
        [Description("Đã kích hoạt")]
        Actived,
        [Description("Chưa kích hoạt")]
        InActived,
    }
    public enum ECustomerType
    {
        [Description("Doanh nghiệp")]
        Company,
        [Description("Cá nhân")]
        Individual,
    }
    public enum EPOStatus
    {
        [Description("Mới")]
        New,
        [Description("Đã tạo đơn tổng hợp")]
        Executed,
    }

    public enum EPAStatus
    {
        [Description("Mới")]
        New,
        [Description("Đã gửi NCC")]
        SendVendor,
        [Description("Hủy")]
        Cancel,
        [Description("Hoàn thành")]
        Completed,
    }

}

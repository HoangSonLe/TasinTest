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
        Product,
        User,
        POStatus,
        PAStatus
    }

    public enum ECommonStatus
    {
        [Description("Đang hoạt động")]
        Actived,
        [Description("Không hoạt động")]
        InActived,
    }
    public enum ECustomerType
    {
        [Description("Doanh nghiệp")]
        Company,
        [Description("Cá nhân")]
        Individual,
    }

    /// <summary>
    /// Processing type enum for products
    /// </summary>
    public enum EProcessingType
    {
        [Description("Nguyên liệu")]
        Material = 1,
        [Description("Sơ chế")]
        SemiProcessed = 2,
        [Description("Thành phẩm")]
        FinishedProduct = 3
    }
    public enum EPOStatus
    {
        [Description("Mới")]
        New,
        [Description("Đã xác nhận")]
        Confirmed,
        [Description("Đã tạo đơn tổng hợp")]
        Executed,
        [Description("Đã hủy")]
        Cancel,
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

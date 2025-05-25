using System.ComponentModel;

namespace Tasin.Website.Common.Enums
{
    /// Rule for naming : E + Noun  

    /// <summary>
    /// Define enums for specific models
    /// </summary>
    public enum EReactType
    {
        Like = 0,
        Dislike = 1,
        Haha = 2
    }
    public enum ERoleType
    {
        [Description("Admin hệ thống")]
        SystemAdmin = 1,
        [Description("Admin")]
        Admin,
        [Description("Người dùng")]
        User,
        [Description("Reporter")]
        Reporter,
    }
    public enum ELevelRole
    {
        SystemAdmin = 1,
        Admin,
        User,
        Reporter,
    }

    public enum EGender
    {
        [Description("Không xác định")]
        UNDEFINE,
        [Description("Nam")]
        MALE,
        [Description("Nữ")]
        FAMALE
    }
    public enum EUrnType
    {
        [Description("Linh")]
        Soul,
        [Description("Cốt")]
        Gauss
    }
    public enum ETelegramNotiType
    {
        [Description("Không xác định")]
        UNDEFINE,
        [Description("Gửi thông báo giỗ")]
        Anniversary,
        [Description("Gửi thông báo hạn ký gửi")]
        Expired,
        [Description("Gửi thông báo Ngày Hiệp Kỵ Chung")]
        GeneralAnniversary,
        [Description("Gửi thông tin web & account")]
        UrlAndAccount,
    }

    public enum EActionRole
    {
        CREATE_USER,
        READ_USER,
        UPDATE_USER,
        DELETE_USER,

        CREATE_VENDOR,
        READ_VENDOR,
        UPDATE_VENDOR,
        DELETE_VENDOR,

        CREATE_CUSTOMER,
        READ_CUSTOMER,
        UPDATE_CUSTOMER,
        DELETE_CUSTOMER,

        CREATE_PRODUCT,
        READ_PRODUCT,
        UPDATE_PRODUCT,
        DELETE_PRODUCT,

        CREATE_UNIT,
        READ_UNIT,
        UPDATE_UNIT,
        DELETE_UNIT,

        CREATE_CATEGORY,
        READ_CATEGORY,
        UPDATE_CATEGORY,
        DELETE_CATEGORY,

        CREATE_TAXRATECONFIG,
        READ_TAXRATECONFIG,
        UPDATE_TAXRATECONFIG,
        DELETE_TAXRATECONFIG,

        CREATE_SPECIALPRODUCTTAXRATE,
        READ_SPECIALPRODUCTTAXRATE,
        UPDATE_SPECIALPRODUCTTAXRATE,
        DELETE_SPECIALPRODUCTTAXRATE,

        CREATE_MATERIAL,
        READ_MATERIAL,
        UPDATE_MATERIAL,
        DELETE_MATERIAL,
        
        CREATE_PROCESSING_TYPE,
        READ_PROCESSING_TYPE,
        UPDATE_PROCESSING_TYPE,
        DELETE_PROCESSING_TYPE,

        CREATE_PURCHASE_ORDER,
        READ_PURCHASE_ORDER,
        UPDATE_PURCHASE_ORDER,
        DELETE_PURCHASE_ORDER,

        CREATE_PURCHASE_AGREEMENT,
        READ_PURCHASE_AGREEMENT,
        UPDATE_PURCHASE_AGREEMENT,
        DELETE_PURCHASE_AGREEMENT,


    }
}

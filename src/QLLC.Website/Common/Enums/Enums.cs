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
        [Description("Reporter")]
        Reporter,
        [Description("Người dùng")]
        User
    }
    public enum ELevelRole
    {
        SystemAdmin = 1,
        Admin,
        Reporter,
        User
    }
    public enum EActionRole
    {
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

}

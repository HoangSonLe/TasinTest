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
        CustomerType
    }
    
    public enum ECustomerType
    {
        [Description("Doanh nghiệp")]
        Company,
        [Description("Cá nhân")]
        Individual,
    }
}

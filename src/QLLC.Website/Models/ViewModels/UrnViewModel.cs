using AutoMapper.Configuration.Annotations;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.Domains.Entitites;
using System.Globalization;

namespace Tasin.Website.Models.ViewModels
{
    public class PreviewLunarDate
    {
        public LunarDate NearestLunarAnniversaryDate { get; set; }
        public DateTime NearestLunarAnniversaryDateToSolarDate { get; set; }
        public double TotalDays { get; set; }
    }
    public class UrnViewModel : BaseAuditableEntity
    {
        public int Id { get; set; } = 0;
        public int TenantId { get; set; } = 0;

        public required string Name { get; set; }
        public required string DharmaName { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DeathDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }

        public PreviewLunarDate PreviewLunarDate => LunarCalendarHelper.CalculateNearestLunarDate(DeathDate.Day, DeathDate.Month);


        public EGender Gender { get; set; }
        public EUrnType UrnType { get; set; }
        public string GenderName => Gender.GetEnumDescription();
        public string UrnTypeName => UrnType.GetEnumDescription();
        public string Note { get; set; }
        public string TowerLocation { get; set; } = "";
        public string CabinetName { get; set; }
        public int RowNumber { get; set; }
        public int BoxNumber { get; set; }
        public int LocationNumber { get; set; }
        public string FileImageUrl { get; set; }
        public string fileImageUrlWithLowQuality { get; set; }
        public bool IsHasImage { get; set; }
        public DateTime ExpiredDate { get; set; }
        public List<int?> FamilyMemberIdList { get; set; }
        public string NameNonUnicode => Name.NonUnicode();
        public string DharmaNameNonUnicode => DharmaName.NonUnicode();
        public string CabinetNameNonUnicode => CabinetName.NonUnicode();
        public string TowerLocationNonUnicode => TowerLocation.NonUnicode();
        public List<UserViewModel> FamilyMemberList { get; set; } = new List<UserViewModel>();
    }
}

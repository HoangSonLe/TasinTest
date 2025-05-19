using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    public class UrnSearchModel : SearchPagingModel<UrnViewModel>
    {
        public string SearchString { get; set; }
        public string SearchNoteString { get; set; }
        /// <summary>
        /// Ngày sinh or mất
        /// </summary>
        public DateTime? FromBirthAndDeathDate { get; set; }
        public DateTime? ToBirthAndDeathDate { get; set; }
        public DateTime? FromExpiredDate { get; set; }
        public DateTime? ToExpiredDate { get; set; }

        public bool IsFilterAnniversary { get; set; }
        public EGender? Gender { get; set; }
        public EUrnType? UrnType { get; set; }
        //public int? RowNumber { get; set; }
        //public int? BoxNumber { get; set; }
    }
}

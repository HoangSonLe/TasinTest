using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.Domains.Entitites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasin.Website.Models.ViewModels
{
    public class StorageMapViewModel : BaseAuditableEntity
    {
        public long Id { get; set; }
        public int TenantId { get; set; }
        public string? Image { get; set; }

        public  string Location { get; set; }
        public  string LocationNonUnicode { get; set; }
        public  string Description { get; set; }

    }
}

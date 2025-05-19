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
    public class ReminderViewModel : BaseAuditableEntity
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public DateTime RemindDate { get; set; }
        public required string Content { get; set; }
        public UserViewModel User { get; set; }
    }
}

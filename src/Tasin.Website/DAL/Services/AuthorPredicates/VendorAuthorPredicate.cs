using LinqKit;
using Tasin.Website.Common.Enums;
using Tasin.Website.Domains.Entitites;
using System.Linq.Expressions;

namespace Tasin.Website.DAL.Services.AuthorPredicates
{
    public static class VendorAuthorPredicate
    {
        public static Expression<Func<Vendor, bool>> GetVendorAuthorPredicate(Expression<Func<Vendor, bool>> predicate, List<ERoleType> roleList, int currentUserId = -1)
        {
            var predicateInner = PredicateBuilder.New<Vendor>(predicate);
            return predicateInner;
        }
    }
}

using LinqKit;
using Tasin.Website.Common.Enums;
using Tasin.Website.Domains.Entitites;
using System.Linq.Expressions;

namespace Tasin.Website.DAL.Services.AuthorPredicates
{
    public static class UrnAuthorPredicate
    {
        public static Expression<Func<Urn,bool>> GetUrnAuthorPredicate(Expression<Func<Urn,bool>> predicate, List<ERoleType> roleList, int? currentTenantId = -1, int currentUserId = -1)
        {
            var predicateInner = PredicateBuilder.New<Urn>(predicate);
            //Query theo từng chùa
            predicateInner = predicateInner.And(i => i.TenantId == currentTenantId);

            //Phân quyền theo role
            if (roleList.Contains(ERoleType.User))
            {
                predicateInner = predicateInner.And(i => i.FamilyMembers.Any(p => p.UserId == currentUserId));
            }
            else if (roleList.Contains(ERoleType.SystemAdmin))
            {
                predicateInner = predicateInner.And(i => false);
            }
            return predicateInner;
        }
    }
}

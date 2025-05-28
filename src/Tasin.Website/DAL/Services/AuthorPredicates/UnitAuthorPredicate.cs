using LinqKit;
using Tasin.Website.Common.Enums;
using Tasin.Website.Domains.Entitites;
using System.Linq.Expressions;

namespace Tasin.Website.DAL.Services.AuthorPredicates
{
    public static class UnitAuthorPredicate
    {
        public static Expression<Func<Unit, bool>> GetUnitAuthorPredicate(Expression<Func<Unit, bool>> predicate, List<ERoleType> roleList, int currentUserId = -1)
        {
            var predicateInner = PredicateBuilder.New<Unit>(predicate);
            
            // Phân quyền theo role
            if (roleList.Contains(ERoleType.SystemAdmin) || roleList.Contains(ERoleType.Admin))
            {
                // Admin có thể xem tất cả
                return predicateInner;
            }
            //else if (roleList.Contains(ERoleType.Reporter))
            //{
            //    // Reporter chỉ có thể xem, không thể sửa
            //    return predicateInner;
            //}
            else if (roleList.Contains(ERoleType.User))
            {
                // User chỉ có thể xem những đơn vị do mình tạo
                if (currentUserId > 0)
                {
                    predicateInner = predicateInner.And(i => i.CreatedBy == currentUserId);
                }
            }
            
            return predicateInner;
        }
    }
}

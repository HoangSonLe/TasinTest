using LinqKit;
using Tasin.Website.Common.Enums;
using Tasin.Website.Domains.Entitites;
using System.Linq.Expressions;

namespace Tasin.Website.DAL.Services.AuthorPredicates
{
    public static class PurchaseOrderAuthorPredicate
    {
        public static Expression<Func<Purchase_Order, bool>> GetPurchaseOrderAuthorPredicate(Expression<Func<Purchase_Order, bool>> predicate, List<ERoleType> roleList, int currentUserId = -1)
        {
            var predicateInner = PredicateBuilder.New<Purchase_Order>(predicate);
            
            // Phân quyền theo role
            if (roleList.Contains(ERoleType.SystemAdmin) || roleList.Contains(ERoleType.Admin))
            {
                // Admin có thể xem tất cả
                return predicateInner;
            }
            else if (roleList.Contains(ERoleType.Reporter))
            {
                // Reporter chỉ có thể xem, không thể sửa
                return predicateInner;
            }
            else if (roleList.Contains(ERoleType.User))
            {
                // User chỉ có thể xem các đơn hàng do mình tạo
                predicateInner = predicateInner.And(i => i.CreatedBy == currentUserId);
                return predicateInner;
            }
            else
            {
                // Các role khác không có quyền xem
                predicateInner = predicateInner.And(i => false);
                return predicateInner;
            }
        }
    }
}

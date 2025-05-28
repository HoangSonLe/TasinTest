using LinqKit;
using Tasin.Website.Common.Enums;
using Tasin.Website.Domains.Entitites;
using System.Linq.Expressions;

namespace Tasin.Website.DAL.Services.AuthorPredicates
{
    public static class CustomerAuthorPredicate
    {
        public static Expression<Func<Customer, bool>> GetCustomerAuthorPredicate(Expression<Func<Customer, bool>> predicate, List<ERoleType> roleList, int currentUserId = -1)
        {
            var predicateInner = PredicateBuilder.New<Customer>(predicate);
            return predicateInner;
        }
    }
}

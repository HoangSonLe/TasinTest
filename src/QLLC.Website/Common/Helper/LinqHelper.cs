using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;

namespace Tasin.Website.Common.Helper
{
    public static class LinqHelper
    {
        public static async Task TrySaveChangesAsync<TEntity, TResult>(
            this Acknowledgement ack,
            Func<IRepository<TEntity>, Task<TResult>> repositoryOperation,
            IRepository<TEntity> respository,
            Action<Exception>? handleError = null
        ) where TEntity : class
        {
            try
            {
                await repositoryOperation(respository);
                ack.IsSuccess = true;
            }
            catch (Exception ex)
            {
                handleError?.Invoke(ex);
                ack.IsSuccess = false;
                ack.ExtractMessage(ex);
            }
        }

        public static DateTime GetDateTimeNow()
        {
            return DateTime.Now.ToUniversalTime();
        }

        public static List<T> ToSingleList<T>(this T data)
        {
            return new List<T>([data]);
        }

        /// <summary>
        /// Performs a search on a string property without considering Vietnamese diacritics (accents)
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="query">The IQueryable to filter</param>
        /// <param name="propertySelector">Expression to select the string property to search on</param>
        /// <param name="searchTerm">The search term (with or without diacritics)</param>
        /// <param name="exactMatch">If true, performs exact match; if false, performs contains match</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> SearchWithoutDiacritics<T>(
            this IQueryable<T> query,
            Expression<Func<T, string>> propertySelector,
            string searchTerm,
            bool exactMatch = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;

            // Remove diacritics from the search term
            string normalizedSearchTerm = Utils.RemoveSignAndLowerCaseVietnameseString(searchTerm);

            // Create a parameter for the entity
            var parameter = propertySelector.Parameters[0];

            // Get the property accessor
            if (propertySelector.Body is not MemberExpression memberExpression)
                return query;

            // Create expression to normalize the property value
            var normalizeMethodCall = Expression.Call(
                typeof(Utils),
                nameof(Utils.RemoveSignAndLowerCaseVietnameseString),
                null,
                memberExpression);

            // Create the comparison expression based on exactMatch parameter
            Expression comparisonExpression;
            if (exactMatch)
            {
                comparisonExpression = Expression.Equal(
                    normalizeMethodCall,
                    Expression.Constant(normalizedSearchTerm));
            }
            else
            {
                var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                if (containsMethod != null)
                {
                    comparisonExpression = Expression.Call(
                        normalizeMethodCall,
                        containsMethod,
                        Expression.Constant(normalizedSearchTerm));
                }
                else
                {
                    // Fallback if Contains method is not found
                    return query;
                }
            }

            // Create the lambda expression for the where clause
            var lambda = Expression.Lambda<Func<T, bool>>(comparisonExpression, parameter);

            // Apply the filter
            return query.Where(lambda);
        }

        /// <summary>
        /// Performs a search on a string property without considering Vietnamese diacritics (accents)
        /// for in-memory collections
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="collection">The collection to filter</param>
        /// <param name="propertySelector">Function to select the string property to search on</param>
        /// <param name="searchTerm">The search term (with or without diacritics)</param>
        /// <param name="exactMatch">If true, performs exact match; if false, performs contains match</param>
        /// <returns>Filtered collection</returns>
        public static IEnumerable<T> SearchWithoutDiacriticsInMemory<T>(
            this IEnumerable<T> collection,
            Func<T, string?> propertySelector,
            string searchTerm,
            bool exactMatch = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return collection;

            // Remove diacritics from the search term
            string normalizedSearchTerm = Utils.RemoveSignAndLowerCaseVietnameseString(searchTerm);

            return collection.Where(item =>
            {
                string? propertyValue = propertySelector(item);

                if (string.IsNullOrEmpty(propertyValue))
                    return false;

                string normalizedPropertyValue = Utils.RemoveSignAndLowerCaseVietnameseString(propertyValue);

                return exactMatch
                    ? normalizedPropertyValue == normalizedSearchTerm
                    : normalizedPropertyValue.Contains(normalizedSearchTerm);
            });
        }
    }
}

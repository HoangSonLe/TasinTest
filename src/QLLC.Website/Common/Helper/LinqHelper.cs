using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.DAL.Interfaces;

namespace Tasin.Website.Common.Helper
{
    public static class LinqHelper
    {
        public static async Task TrySaveChangesAsync<TEntity,TResult>(
            this Acknowledgement ack,
            Func<IRepository<TEntity>, Task<TResult>> repositoryOperation, 
            IRepository<TEntity> respository, 
            Action<Exception> handleError = null
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
            return new List<T> { data };
        }
    }
}

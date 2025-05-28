using Microsoft.EntityFrameworkCore.Storage;

namespace Tasin.Website.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <returns>Database transaction</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Commit all changes within the current transaction
        /// </summary>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Execute multiple operations within a transaction
        /// </summary>
        /// <param name="operations">Operations to execute</param>
        /// <returns>Result of the transaction</returns>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operations);

        /// <summary>
        /// Execute multiple operations within a transaction (void return)
        /// </summary>
        /// <param name="operations">Operations to execute</param>
        Task ExecuteInTransactionAsync(Func<Task> operations);

        // Repository access
        IPurchaseAgreementRepository PurchaseAgreements { get; }
        IPurchaseAgreementItemRepository PurchaseAgreementItems { get; }
        IPurchaseOrderRepository PurchaseOrders { get; }
        IPurchaseOrderItemRepository PurchaseOrderItems { get; }
        IVendorRepository Vendors { get; }
        IProduct_VendorRepository ProductVendors { get; }
    }
}

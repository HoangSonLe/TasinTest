using Microsoft.EntityFrameworkCore.Storage;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;

namespace Tasin.Website.DAL.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SampleDBContext _context;
        private readonly SampleReadOnlyDBContext _readOnlyContext;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private IPurchaseAgreementRepository? _purchaseAgreements;
        private IPurchaseAgreementItemRepository? _purchaseAgreementItems;
        private IPurchaseOrderRepository? _purchaseOrders;
        private IPurchaseOrderItemRepository? _purchaseOrderItems;
        private IVendorRepository? _vendors;
        private IProduct_VendorRepository? _productVendors;

        public UnitOfWork(SampleDBContext context, SampleReadOnlyDBContext readOnlyContext)
        {
            _context = context;
            _readOnlyContext = readOnlyContext;
        }

        // Lazy loading repositories
        public IPurchaseAgreementRepository PurchaseAgreements =>
            _purchaseAgreements ??= new PurchaseAgreementRepository(_context, _readOnlyContext);

        public IPurchaseAgreementItemRepository PurchaseAgreementItems =>
            _purchaseAgreementItems ??= new PurchaseAgreementItemRepository(_context, _readOnlyContext);

        public IPurchaseOrderRepository PurchaseOrders =>
            _purchaseOrders ??= new PurchaseOrderRepository(_context, _readOnlyContext);

        public IPurchaseOrderItemRepository PurchaseOrderItems =>
            _purchaseOrderItems ??= new PurchaseOrderItemRepository(_context, _readOnlyContext);

        public IVendorRepository Vendors =>
            _vendors ??= new VendorRepository(_context, _readOnlyContext);

        public IProduct_VendorRepository ProductVendors =>
            _productVendors ??= new Product_VendorRepository(_context, _readOnlyContext);

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operations)
        {
            using var transaction = await BeginTransactionAsync();
            try
            {
                var result = await operations();
                await CommitTransactionAsync();
                return result;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operations)
        {
            using var transaction = await BeginTransactionAsync();
            try
            {
                await operations();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
            _readOnlyContext?.Dispose();
        }
    }
}

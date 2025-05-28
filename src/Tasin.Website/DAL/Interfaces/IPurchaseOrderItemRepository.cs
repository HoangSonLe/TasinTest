using Tasin.Website.DAL.Repository;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Interfaces
{
    public interface IPurchaseOrderItemRepository : IRepositoryGenerator<Purchase_Order_Item>
    {
        Task<List<Purchase_Order_Item>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
        Task DeleteByPurchaseOrderIdAsync(int purchaseOrderId);
    }
}

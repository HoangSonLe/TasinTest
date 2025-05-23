using Microsoft.EntityFrameworkCore;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class PurchaseOrderItemRepository : RepositoryGenerator<Purchase_Order_Item>, IPurchaseOrderItemRepository
    {
        public PurchaseOrderItemRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext)
            : base(context, readOnlyDBContext)
        {
        }

        public async Task<List<Purchase_Order_Item>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            return await ReadOnlyRespository.GetAsync(
                filter: item => item.PO_ID == purchaseOrderId,
                orderBy: q => q.OrderBy(i => i.ID)
            );
        }

        public async Task DeleteByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            // Since Purchase_Order_Item doesn't inherit from BaseAuditableEntity and doesn't have IsActive property,
            // we need to actually delete the records
            var items = await ReadOnlyRespository.GetAsync(
                filter: item => item.PO_ID == purchaseOrderId
            );

            if (items.Any())
            {
                foreach (var item in items)
                {
                    await Repository.DeleteAsync(item);
                }
            }
        }
    }
}

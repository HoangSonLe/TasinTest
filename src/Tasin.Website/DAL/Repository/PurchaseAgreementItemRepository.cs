using Microsoft.EntityFrameworkCore;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class PurchaseAgreementItemRepository : RepositoryGenerator<Purchase_Agreement_Item>, IPurchaseAgreementItemRepository
    {
        public PurchaseAgreementItemRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext)
            : base(context, readOnlyDBContext)
        {
        }

        public async Task<List<Purchase_Agreement_Item>> GetByPurchaseAgreementIdAsync(int purchaseAgreementId)
        {
            return await ReadOnlyRespository.GetAsync(
                filter: item => item.PA_ID == purchaseAgreementId,
                orderBy: q => q.OrderBy(i => i.Product_ID)
            );
        }

        public async Task DeleteByPurchaseAgreementIdAsync(int purchaseAgreementId)
        {
            var items = await Repository.GetAsync(filter: item => item.PA_ID == purchaseAgreementId);
            foreach (var item in items)
            {
                await Repository.DeleteAsync(item);
            }
        }
    }
}

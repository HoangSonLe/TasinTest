using Tasin.Website.DAL.Repository;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Interfaces
{
    public interface IPurchaseAgreementItemRepository : IRepositoryGenerator<Purchase_Agreement_Item>
    {
        Task<List<Purchase_Agreement_Item>> GetByPurchaseAgreementIdAsync(int purchaseAgreementId);
        Task DeleteByPurchaseAgreementIdAsync(int purchaseAgreementId);
    }
}

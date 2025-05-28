using Tasin.Website.DAL.Repository;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Interfaces
{
    public interface IProduct_VendorRepository : IRepositoryGenerator<Product_Vendor>
    {
        Task<Product_Vendor?> GetByProductIdAsync(int productId);
        Task<List<Product_Vendor>> GetByProductIdsAsync(List<int> productIds);
    }
}

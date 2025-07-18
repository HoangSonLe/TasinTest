using Microsoft.EntityFrameworkCore;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class Product_VendorRepository : RepositoryGenerator<Product_Vendor>, IProduct_VendorRepository
    {
        public Product_VendorRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext)
            : base(context, readOnlyDBContext)
        {
        }

        public async Task<Product_Vendor?> GetByProductIdAsync(int productId)
        {
            var productVendors = await ReadOnlyRespository.GetAsync(
                filter: pv => pv.Product_ID == productId,
                orderBy: q => q.OrderBy(pv => pv.Priority ?? int.MaxValue)
            );
            return productVendors.FirstOrDefault();
        }

        public async Task<List<Product_Vendor>> GetByProductIdsAsync(List<int> productIds)
        {
            return await ReadOnlyRespository.GetAsync(
                filter: pv => productIds.Contains(pv.Product_ID),
                orderBy: q => q.OrderBy(pv => pv.Priority ?? int.MaxValue)
            );
        }

        public async Task<List<Product_Vendor>> GetHighestPriorityVendorsByProductIdsAsync(List<int> productIds)
        {
            var allProductVendors = await ReadOnlyRespository.GetAsync(
                filter: pv => productIds.Contains(pv.Product_ID),
                orderBy: q => q.OrderBy(pv => pv.Priority ?? int.MaxValue)
            );

            // Group by Product_ID and take the vendor with highest priority (lowest priority number) for each product
            return allProductVendors
                .GroupBy(pv => pv.Product_ID)
                .Select(g => g.OrderBy(pv => pv.Priority ?? int.MaxValue).ThenBy(e=>e.UnitPrice).First())
                .ToList();
        }
    }
}

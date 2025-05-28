using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    /// <summary>
    /// Interface for product order statistics service
    /// </summary>
    public interface IProductOrderStatisticsService
    {
        /// <summary>
        /// Get product order statistics grouped by vendor for completed PAs
        /// </summary>
        /// <param name="searchModel">Search parameters including filters</param>
        /// <returns>Statistics grouped by vendor</returns>
        Task<Acknowledgement<JsonResultPaging<List<ProductOrderStatisticsViewModel>>>> GetProductOrderStatistics(ProductOrderStatisticsSearchModel searchModel);

        /// <summary>
        /// Get customer product order statistics grouped by customer for confirmed POs
        /// </summary>
        /// <param name="searchModel">Search parameters including filters</param>
        /// <returns>Statistics grouped by customer</returns>
        Task<Acknowledgement<JsonResultPaging<List<CustomerProductOrderStatisticsViewModel>>>> GetCustomerProductOrderStatistics(CustomerProductOrderStatisticsSearchModel searchModel);
    }
}

using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IPurchaseOrderService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<PurchaseOrderViewModel>>>> GetPurchaseOrderList(PurchaseOrderSearchModel postData);
        Task<Acknowledgement<PurchaseOrderViewModel>> GetPurchaseOrderById(int purchaseOrderId);
        Task<Acknowledgement> CreateOrUpdatePurchaseOrder(PurchaseOrderViewModel postData);
        Task<Acknowledgement> DeletePurchaseOrderById(int purchaseOrderId);
        Task<Acknowledgement> CancelPurchaseOrderById(int purchaseOrderId);
    }
}

using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IPurchaseAgreementService : IBaseService, IDisposable
    {
        // Individual PA methods (Child PAs)
        Task<Acknowledgement<JsonResultPaging<List<PurchaseAgreementViewModel>>>> GetPurchaseAgreementList(PurchaseAgreementSearchModel postData);
        Task<Acknowledgement<PurchaseAgreementViewModel>> GetPurchaseAgreementById(int purchaseAgreementId);
        Task<Acknowledgement> UpdatePurchaseAgreement(PurchaseAgreementViewModel postData);
        Task<Acknowledgement> DeletePurchaseAgreementById(int purchaseAgreementId);

        // PA Group methods (Parent PAs)
        Task<Acknowledgement<JsonResultPaging<List<PAGroupViewModel>>>> GetPAGroupList(PAGroupSearchModel postData);
        Task<Acknowledgement<PAGroupViewModel>> GetPAByGroupCode(string groupCode);
        Task<Acknowledgement<PAGroupViewModel>> GetPAGroupPreview();
        Task<Acknowledgement<EditablePAGroupPreviewViewModel>> GetEditablePAGroupPreview();
        Task<Acknowledgement<PAGroupViewModel>> CreatePAGroup();
        Task<Acknowledgement<PAGroupViewModel>> CreatePAGroupWithCustomMapping(CreatePAGroupWithMappingRequest request);
        Task<Acknowledgement> CompletePAGroup(string groupCode);
        Task<Acknowledgement> CancelPAGroup(string groupCode);
    }
}

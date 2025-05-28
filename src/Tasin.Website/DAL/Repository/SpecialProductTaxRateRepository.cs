using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class SpecialProductTaxRateRepository : RepositoryGenerator<SpecialProductTaxRate>, ISpecialProductTaxRateRepository
    {
        public SpecialProductTaxRateRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext) : base(context, readOnlyDBContext)
        {
        }
    }
}

using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class UnitRepository : RepositoryGenerator<Unit>, IUnitRepository
    {
        public UnitRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext) : base(context, readOnlyDBContext)
        {
        }
    }
}

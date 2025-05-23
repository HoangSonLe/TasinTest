using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class MaterialRepository : RepositoryGenerator<Material>, IMaterialRepository
    {
        public MaterialRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext) : base(context, readOnlyDBContext)
        {
        }
    }
}

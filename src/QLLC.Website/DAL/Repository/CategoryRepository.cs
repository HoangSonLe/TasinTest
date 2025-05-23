using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Repository
{
    public class CategoryRepository : RepositoryGenerator<Category>, ICategoryRepository
    {
        public CategoryRepository(SampleDBContext context, SampleReadOnlyDBContext readOnlyDBContext) : base(context, readOnlyDBContext)
        {
        }
    }
}

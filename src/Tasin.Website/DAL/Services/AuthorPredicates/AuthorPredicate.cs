//using Tasin.Website.DAL.Repository;
//using Tasin.Website.Domains.Entitites;
//using Tasin.Website.Models.ViewModels;

//namespace Tasin.Website.DAL.Services.AuthorPredicates
//{
//    public static class AuthorPredicate
//    {
//        /// <summary>
//        /// Adds author information (created by and updated by names) to a list of view models
//        /// </summary>
//        public static async Task AddAuthorInfo<T>(List<T> viewModels, IReadOnlyRepositoryGenerator<User> userRepository) where T : BaseViewModel
//        {
//            if (viewModels == null || !viewModels.Any())
//                return;

//            // Get all user IDs from the view models
//            var userIds = new List<int>();
//            foreach (var viewModel in viewModels)
//            {
//                if (viewModel.CreatedBy > 0 && !userIds.Contains(viewModel.CreatedBy))
//                    userIds.Add(viewModel.CreatedBy);

//                if (viewModel.UpdatedBy.HasValue && viewModel.UpdatedBy.Value > 0 && !userIds.Contains(viewModel.UpdatedBy.Value))
//                    userIds.Add(viewModel.UpdatedBy.Value);
//            }

//            if (!userIds.Any())
//                return;

//            // Get all users in one query
//            var users = await userRepository.GetAsync(u => userIds.Contains(u.Id));

//            // Add author information to each view model
//            foreach (var viewModel in viewModels)
//            {
//                var createdByUser = users.FirstOrDefault(u => u.Id == viewModel.CreatedBy);
//                var updatedByUser = viewModel.UpdatedBy.HasValue ? users.FirstOrDefault(u => u.Id == viewModel.UpdatedBy.Value) : null;

//                viewModel.CreatedByName = createdByUser?.Name ?? "Unknown";
//                viewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";
//            }
//        }
//    }
//}

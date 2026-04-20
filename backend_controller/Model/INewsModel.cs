using vizsgaController.Dtos;

namespace vizsgaController.Model
{
    public interface INewsModel
    {
        // feed / post detail
        public IEnumerable<PostFeedDTO> GetFeed(int page, int pageSize, string? category);
        public PostFeedDTO? GetPostById(int id);
        public IEnumerable<CommentResponseDTO> GetPostComments(int id);

        //for loading
        public IEnumerable<DisplayAllUserDTO> GetAllUsers();
        public IEnumerable<DisplayAllPostsDTO> GetAllPosts();
        public IEnumerable<CategoryDTO> GetAllCategories();
        //search
        public IEnumerable<UsersBySearch> GetUserNamesBySearch(string name);
        public IEnumerable<PostsBySearch> GetPostsBySearchAndCat(string title, string cat);
        public IEnumerable<PostsByCat> GetPostsByCategory(string cats);
        public IEnumerable<GetCommentsFromPost> PostCommentsFetch(int id);
        public IEnumerable<OwnPosts> GetOwnPosts(int id);
        public IEnumerable<OwnComments> GetOwnComments(int id);
        public IEnumerable<PostsFromOwnComment> GetPostsFromOwnComments(int id);
        public IEnumerable<LikedPosts> GetLikedPosts(int id);
        public IEnumerable<DislikedPosts> GetDislikedPosts(int id);
        public IEnumerable<Favorites> GetFavoritePosts(int id);


        //users
        public Task DeleteUsers(int id);
        public Task ModifyUsers(ModifyUserDTO  source);
        //posts
        public Task CreatePost(PostDTO source);
        public Task DeletePost(int id);
        public Task DeleteOwnPost(DeleteOwnPostDTO source);
        public Task FavouritePost(FavouritePostDTO source);
        public Task voteOnPost(VoteDTO source);
        //coment
        public Task CommentOnPost(CommentDTO source);
        public Task DeleteComments(int id);
        //categories
        public Task CreateCategory(CategoryDTO source);
        public Task DeleteCategory(int id);
        //reports
        public IEnumerable<OwnReports> GetOwnReports(int id);
        public IEnumerable<AdminReportDTO> GetAllReports();
        public Task CreateReport(ReportDTO source);
        public Task ResolveReportKeepPost(int reportId);
        public Task ResolveReportDeletePost(int reportId);
        

    }
}

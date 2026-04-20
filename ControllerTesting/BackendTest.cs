using vizsgaController.Dtos;
using vizsgaController.Model;
using vizsgaController.Persistence;

namespace ControllerTesting
{
    public class BackendTest
    {
        private readonly NewsModel _model;
        private readonly NewsDbContext _context;

        public BackendTest()
        {
            _context = DbContextFactory.Create();
            _model = new NewsModel(_context);
        }

        [Fact]
        public void NameSearch_Valid()
        {
            var result = _model.GetUserNamesBySearch("admin").ToList();
            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.Contains("admin", x.Username));
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NameSearch_EmptyParam(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() => _model.GetUserNamesBySearch(name!).ToList());
            Assert.Contains("pretty", ex.Message);
        }
        [Fact]
        public void NameSearch_NoMatch()
        {
            var ex = Assert.Throws<InvalidDataException>(() => _model.GetUserNamesBySearch("NINCSILYEN").ToList());
            Assert.Contains("No user", ex.Message);
        }
        ///////////////////////////////////////

        [Fact]
        public void PostSearch_Valid()
        {
            var result = _model.GetPostsBySearchAndCat("future", "All").ToList();
            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.Contains("Future", x.Title));
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void PostSearch_EmptyTitle(string? title)
        {
            var ex = Assert.Throws<ArgumentException>(() => _model.GetPostsBySearchAndCat(title!, "Sports").ToList());
            Assert.Contains("title", ex.Message);
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void PostSearch_EmptyCat(string? cat)
        {
            var ex = Assert.Throws<ArgumentException>(() => _model.GetPostsBySearchAndCat("future", cat!).ToList());
            Assert.Contains("cat", ex.Message);
        }
        //[Fact]
        //  public void PostSearch_NoMatchForTitle()
        //  {
        //     var ex = Assert.Throws<InvalidDataException>(() => _model.GetPostsBySearch("NINCSILYEN").ToList());
        //    Assert.Contains("No post", ex.Message);
        // }
        ///////////////////////////////////////

        [Fact]
        public async Task DeleteUser_Valid()
        {
            var id = _context.Users
                .Where(r => r.Username == "john_doe")
                .Select(r => r.UserID)
                .First();

            var before = _context.Users.Count();

            await _model.DeleteUsers(id);

            var after = _context.Users.Count();
            Assert.Equal(before - 1, after);
            Assert.False(_context.Users.Any(r => r.UserID == id));
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteUsers_IDOutOfRange(int id)
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.DeleteUsers(id));
        }
        [Fact]
        public async Task DeleteUsers_IDNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteUsers(999999));
        }
        //////////////////////////////////////////

        [Fact]
        public async Task ModifyUser_Valid()
        {
            var id = _context.Users
                .Where(r => r.Username == "admin")
                .Select(r => r.UserID)
                .First();

            var dto = new ModifyUserDTO
            {
                id = id,
                name = "adminreal"
            };

            await _model.ModifyUsers(dto);

            var updated = _context.Users.First(r => r.UserID == id);
            Assert.Equal(dto.name, updated.Username);
        }

        [Fact]
        public async Task ModifyUser_NullDTO()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _model.ModifyUsers(null!));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ModifyUser_IDOutOfRange(int id)
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.DeleteUsers(id));
        }
        [Fact]
        public async Task ModifyUser_IDNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteUsers(999999));
        }
        [Fact]
        public async Task ModifyUser_EmptyName()
        {
            var id = _context.Users.Select(r => r.UserID).First();

            var dto = new ModifyUserDTO
            {
                id = id,
                name = "   "
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _model.ModifyUsers(dto));
        }

        [Fact]
        public async Task ModifyUser_UsernameExists()
        {
            var id = _context.Users.Select(r => r.UserID).First();

            var dto = new ModifyUserDTO
            {
                id = id,
                name = "jane_smith"
            };
            var before = _context.Users.Count();
            await _model.DeleteUsers(id);

            var after = _context.Users.Count();
            Assert.Equal(before - 1, after);
            Assert.False(_context.Users.Any(r => r.UserID == id));
        }
        ///////////////////////////////////////////


        [Fact]
        public async Task CreatePost_Valid()
        {
            var dto = new PostDTO
            {
                categoryname = "Politics",
                content = "This is a test post.",
                created_at = DateTime.Now,
                title = "Test Post",
                userID = 1
            };

            var before = _context.Posts.Count();
            await _model.CreatePost(dto);
            var after = _context.Posts.Count();
            Assert.Equal(before + 1, after);
        }

        [Fact]
        public async Task CreatePost_NullDTO()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _model.CreatePost(null!));
        }

        [Fact]
        public async Task CreatePost_InvalidId()
        {
            var dto = new PostDTO
            {
                categoryname = "",
                content = "This is a test post.",
                created_at = DateTime.Now,
                title = "test",
                userID = -1
            };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.CreatePost(dto));
        }

        [Fact]
        public async Task CreatePost_UserNotFound()
        {
            var dto = new PostDTO
            {
                categoryname = "",
                content = "This is a test post.",
                created_at = DateTime.Now,
                title = "test",
                userID = 999999999
            };
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.CreatePost(dto));
        }

        [Fact]
        public async Task CreatePost_CategoryNotFound()
        {
            var dto = new PostDTO
            {
                categoryname = "",
                content = "This is a test post.",
                created_at = DateTime.Now,
                title = "test",
                userID = 1
            };
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.CreatePost(dto));
        }

        [Fact]
        public async Task CreatePost_EmptyTitle()
        {
            var dto = new PostDTO
            {
                categoryname = "Politics",
                content = "This is a test post.",
                created_at = DateTime.Now,
                title = "   ",
                userID = 1
            };
            await Assert.ThrowsAsync<ArgumentException>(() => _model.CreatePost(dto));
        }

        [Fact]
        public async Task CreatePost_EmptyContent()
        {
            var dto = new PostDTO
            {
                categoryname = "Politics",
                content = "   ",
                created_at = DateTime.Now,
                title = "test",
                userID = 1
            };
            await Assert.ThrowsAsync<ArgumentException>(() => _model.CreatePost(dto));
        }
        ///////////////////////////////////////////
        [Fact]
        public async Task DeletePost_Valid()
        {
            var id = _context.Posts.Select(r => r.PostID).First();
            var before = _context.Posts.Count();
            await _model.DeletePost(id);
            var after = _context.Posts.Count();
            Assert.Equal(before - 1, after);
            Assert.False(_context.Posts.Any(r => r.PostID == id));
        }

        [Fact]
        public async Task DeletePost_InvalidId()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.DeletePost(-1));
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task DeleteOwnPost_Valid()
        {
            var dto = new DeleteOwnPostDTO
            {
                userId = 1,
                postid = 1
            };
            int before = _context.Posts.Count();
            await _model.DeleteOwnPost(dto);
            int after = _context.Posts.Count();
            Assert.Equal(before - 1, after);
        }

        [Fact]
        public async Task DeleteOwnPost_UserNotFound()
        {
            var dto = new DeleteOwnPostDTO
            {
                postid = 1,
                userId = 999999
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.DeleteOwnPost(dto));
        }

        [Fact]
        public async Task DeleteOwnPost_PostNotFound()
        {
            var dto = new DeleteOwnPostDTO
            {
                postid = 999999,
                userId = 1
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.DeleteOwnPost(dto));
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task FavouritePost_valid()
        {
            var dto = new FavouritePostDTO
            {
                postId = 1,
                userId = 1
            };
            var before = _context.Users.Where(r => r.UserID == dto.userId).Select(r => r.Favourites.Count).First();
            await _model.FavouritePost(dto);
            var after = _context.Users.Where(r => r.UserID == dto.userId).Select(r => r.Favourites.Count).First();
            Assert.Equal(before + 1, after);
        }

        [Fact]

        public async Task FavouritePost_UserNotFound()
        {
            var dto = new FavouritePostDTO
            {
                postId = 1,
                userId = 999999
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.FavouritePost(dto));
        }

        [Fact]
        public async Task FavouritePost_PostNotFound()
        {
            var dto = new FavouritePostDTO
            {
                postId = 999999,
                userId = 1
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.FavouritePost(dto));
        }

        ///////////////////////////////////////////

        [Fact]

        public async Task VoteOnPost_Valid()
        {
            var dto = new VoteDTO
            {
                postId = 1,
                userId = 1,
                isUpvote = true
            };

            var before = _context.Posts.Where(r => r.PostID == dto.postId).Select(r => r.Votes).First();
            await _model.voteOnPost(dto);
            var after = _context.Posts.Where(r => r.PostID == dto.postId).Select(r => r.Votes).First();
            Assert.Equal(before + 1, after);
        }

        [Fact]
        public async Task VoteOnPost_SwitchUpvoteToDownvote_DecreasesByTwo()
        {
            var upvote = new VoteDTO
            {
                postId = 1,
                userId = 1,
                isUpvote = true
            };

            var downvote = new VoteDTO
            {
                postId = 1,
                userId = 1,
                isUpvote = false
            };

            var before = _context.Posts.Where(r => r.PostID == upvote.postId).Select(r => r.Votes).First();
            await _model.voteOnPost(upvote);
            await _model.voteOnPost(downvote);
            var after = _context.Posts.Where(r => r.PostID == upvote.postId).Select(r => r.Votes).First();

            Assert.Equal(before - 1, after);
        }

        [Fact]
        public async Task VoteOnPost_SwitchDownvoteToUpvote_IncreasesByTwo()
        {
            var downvote = new VoteDTO
            {
                postId = 1,
                userId = 1,
                isUpvote = false
            };

            var upvote = new VoteDTO
            {
                postId = 1,
                userId = 1,
                isUpvote = true
            };

            var before = _context.Posts.Where(r => r.PostID == upvote.postId).Select(r => r.Votes).First();
            await _model.voteOnPost(downvote);
            await _model.voteOnPost(upvote);
            var after = _context.Posts.Where(r => r.PostID == upvote.postId).Select(r => r.Votes).First();

            Assert.Equal(before + 1, after);
        }

        [Fact]
        public async Task VoteOnPost_UserNotFound()
        {
            var dto = new VoteDTO
            {
                postId = 1,
                userId = 999999,
                isUpvote = true
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.voteOnPost(dto));
        }

        [Fact]
        public async Task VoteOnPost_PostNotFound()
        {
            var dto = new VoteDTO
            {
                postId = 999999,
                userId = 1,
                isUpvote = true
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.voteOnPost(dto));
        }

        ///////////////////////////////////////////
        [Fact]
        public async Task CommentOnPost_Valid()
        {
            var dto = new CommentDTO
            {
                postID = 1,
                userID = 1,
                commentcontent = "This is a test comment."
            };
            var before = _context.Posts.Where(r => r.PostID == dto.postID).Select(r => r.Comments.Count).First();
            await _model.CommentOnPost(dto);
            var after = _context.Posts.Where(r => r.PostID == dto.postID).Select(r => r.Comments.Count).First();
            Assert.Equal(before + 1, after);
        }
        [Fact]
        public async Task CommentOnPost_UserNotFound()
        {
            var dto = new CommentDTO
            {
                postID = 1,
                userID = 999999,
                commentcontent = "This is a test comment."
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.CommentOnPost(dto));
        }
        [Fact]
        public async Task CommentOnPost_PostNotFound()
        {
            var dto = new CommentDTO
            {
                postID = 999999,
                userID = 1,
                commentcontent = "This is a test comment."
            };
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _model.CommentOnPost(dto));

        }

        ///////////////////////////////////////////
        [Fact]
        public async Task CreateCategory_Valid()
        {
            var dto = new CategoryDTO
            {
                categoryname = "Test Category"
            };
            var before = _context.Categories.Count();
            await _model.CreateCategory(dto);
            var after = _context.Categories.Count();
            Assert.Equal(before + 1, after);
        }

        [Fact]
        public async Task CreateCategory_NullDTO()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _model.CreateCategory(null!));
        }

        [Fact]
        public async Task CreateCategory_EmptyName()
        {
            var dto = new CategoryDTO
            {
                categoryname = "   "
            };
            await Assert.ThrowsAsync<ArgumentException>(() => _model.CreateCategory(dto));
        }

        ///////////////////////////////////////////
        [Fact]
        public async Task DeleteCategory_Valid()
        {
            var id = _context.Categories.Select(r => r.CategoryID).First();
            var before = _context.Categories.Count();
            await _model.DeleteCategory(id);
            var after = _context.Categories.Count();
            Assert.Equal(before - 1, after);
            Assert.False(_context.Categories.Any(r => r.CategoryID == id));
        }

        [Fact]
        public async Task DeleteCategory_InvalidId()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteCategory(-1));
        }

        [Fact]
        public async Task DeleteCategory_CategoryNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteCategory(999999));

        }

        ///////////////////////////////////////////

        [Fact]
        public async Task CreateReport_Valid()
        {
            var dto = new ReportDTO
            {
                postID = 1,
                userID = 1,
                reportreason = "This is a test report.",
                reportcreated_at = DateTime.Now
            };
            var before = _context.Reports.Count();
            await _model.CreateReport(dto);
            var after = _context.Reports.Count();
            Assert.Equal(before + 1, after);
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetFeed_Valid()
        {
            var result = _model.GetFeed(1, 10, "All").ToList();
            Assert.NotNull(result);
            Assert.True(result.Count() <= 10);
            Assert.All(result, x => Assert.NotNull(x.Title));
        }

        [Fact]
        public async Task GetFeed_InvalidPage()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetFeed(-1, 10, "All").ToList());
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetPostById_Valid()
        {
            var result = _model.GetPostById(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPostById_InvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetPostById(-1));
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetPostComments_Valid()
        {
            var result = _model.GetPostComments(1).ToList();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPostComments_InvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetPostComments(-1).ToList());
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetAllUsers_Valid()
        {
            var result = _model.GetAllUsers().ToList();
            Assert.NotNull(result);
            Assert.All(result, x => Assert.NotNull(x.Username));
            Assert.All(result, x => Assert.NotNull(x.UserID));
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetAllPosts_Valid()
        {
            var res = _model.GetAllPosts().ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.Title));
            Assert.All(res, x => Assert.NotNull(x.Content));
            Assert.All(res, x => Assert.NotNull(x.Created_at));
            Assert.All(res, x => Assert.NotNull(x.UserID));
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetAllCategories_Valid()
        {
            var res = _model.GetAllCategories().ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.categoryname));
            Assert.All(res, x => Assert.NotNull(x.categoryID));
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetPostsByCat_Valid()
        {
            var res = _model.GetPostsByCategory("Technology").ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.Title));
            Assert.All(res, x => Assert.NotNull(x.UserID));
            Assert.All(res, x => Assert.NotNull(x.Content));
            Assert.All(res, x => Assert.NotNull(x.Created_at));
            Assert.All(res, x => Assert.NotNull(x.Votes));
        }

        [Fact]
        public async Task GetPostsByCat_InvalidCat()
        {
            Assert.Throws<InvalidDataException>(() => _model.GetPostsByCategory("NINCSILYEN").ToList());
        }

        [Fact]
        public async Task GetPostsByCat_EmptyCat()
        {
            Assert.Throws<ArgumentException>(() => _model.GetPostsByCategory("   ").ToList());
        }
        ///////////////////////////////////////////

        [Fact]
        public async Task GetOwnPosts_Valid()
        {
            var res = _model.GetOwnPosts(1).ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.Title));
            Assert.All(res, x => Assert.NotNull(x.UserID));
            Assert.All(res, x => Assert.NotNull(x.Content));
            Assert.All(res, x => Assert.NotNull(x.Created_at));
            Assert.All(res, x => Assert.NotNull(x.Votes));
        }

        [Fact]
        public async Task GetOwnPosts_UserNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => _model.GetOwnPosts(999999).ToList());
        }

        [Fact]
        public async Task GetOwnPosts_InvalidUserId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetOwnPosts(-1).ToList());
        }

        ///////////////////////////////////////////
        [Fact]
        public async Task GetOwnComments_Valid()
        {
            var res = _model.GetOwnComments(1).ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.CommentContent));
            Assert.All(res, x => Assert.NotNull(x.CommentCreated_at));
        }

        [Fact]
        public async Task GetOwnComments_UserNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => _model.GetOwnComments(999999).ToList());
        }

        [Fact]
        public async Task GetOwnComments_InvalidUserId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetOwnComments(-1).ToList());
        }

        ///////////////////////////////////////////

        [Fact]
        public async Task GetLikedPosts_Valid()
        {
            var res = _model.GetLikedPosts(1).ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.Title));
            Assert.All(res, x => Assert.NotNull(x.UserID));
            Assert.All(res, x => Assert.NotNull(x.Content));
            Assert.All(res, x => Assert.NotNull(x.Created_at));
            Assert.All(res, x => Assert.NotNull(x.Votes));
        }

        [Fact]
        public async Task GetLikedPosts_UserNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => _model.GetLikedPosts(999999).ToList());
        }
        [Fact]
        public async Task GetLikedPosts_InvalidUserId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetLikedPosts(-1).ToList());
        }

        ///////////////////////////////////////////
        [Fact]
        public async Task GetDisLikedPosts_Valid()
        {
            var res = _model.GetDislikedPosts(1).ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.Title));
            Assert.All(res, x => Assert.NotNull(x.UserID));
            Assert.All(res, x => Assert.NotNull(x.Content));
            Assert.All(res, x => Assert.NotNull(x.Created_at));
            Assert.All(res, x => Assert.NotNull(x.Votes));
        }

        [Fact]
        public async Task GetDisLikedPosts_UserNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => _model.GetDislikedPosts(999999).ToList());
        }
        [Fact]
        public async Task GetDisLikedPosts_InvalidUserId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetDislikedPosts(-1).ToList());
        }

        ///////////////////////////////////////////
        [Fact]
        public async Task GetFavouritePosts_Valid()
        {
            var res = _model.GetFavoritePosts(1).ToList();
            Assert.NotNull(res);
            Assert.All(res, x => Assert.NotNull(x.Title));
            Assert.All(res, x => Assert.NotNull(x.UserID));
            Assert.All(res, x => Assert.NotNull(x.Content));
            Assert.All(res, x => Assert.NotNull(x.Created_at));
            Assert.All(res, x => Assert.NotNull(x.Votes));
        }

        [Fact]
        public async Task GetFavoritePosts_UserNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => _model.GetFavoritePosts(999999).ToList());
        }
        [Fact]
        public async Task GetFavoritePosts_InvalidUserId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.GetFavoritePosts(-1).ToList());
        }
    }
}
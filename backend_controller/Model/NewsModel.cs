using Microsoft.EntityFrameworkCore;
using vizsgaController.Dtos;
using vizsgaController.Persistence;

namespace vizsgaController.Model
{
    public class NewsModel : INewsModel
    {
        private readonly NewsDbContext _context;
        private const string ReportStatusOpen = "Open";
        private const string ReportStatusClosedNotDeleted = "Closed (not deleted)";
        private const string ReportStatusClosedDeleted = "Closed (deleted)";

        public NewsModel(NewsDbContext context)
        {
            _context = context;
        }

        private static bool IsOpenReportStatus(string? reportStatus)
        {
            return string.Equals(reportStatus, ReportStatusOpen, StringComparison.OrdinalIgnoreCase)
                || string.Equals(reportStatus, "Pending", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeReportStatus(string? reportStatus)
        {
            if (IsOpenReportStatus(reportStatus)) return ReportStatusOpen;
            if (string.Equals(reportStatus, ReportStatusClosedNotDeleted, StringComparison.OrdinalIgnoreCase)) return ReportStatusClosedNotDeleted;
            if (string.Equals(reportStatus, ReportStatusClosedDeleted, StringComparison.OrdinalIgnoreCase)) return ReportStatusClosedDeleted;
            return reportStatus ?? ReportStatusOpen;
        }

        //csabi methods???
        public IEnumerable<PostFeedDTO> GetFeed(int page, int pageSize, string? category)
        {
            if (page < 0 || pageSize <= 0) throw new ArgumentOutOfRangeException("Invalid pagination values.");
            return _context.Posts
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Comments)
                .Where(x => category == null || (x.Category != null && x.Category.Categoryname == category))
                .OrderByDescending(x => x.Created_at)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(x => new PostFeedDTO
                {
                    PostID = x.PostID,
                    Title = x.Title,
                    Content = x.Content,
                    Created_at = x.Created_at,
                    Votes = x.Votes,
                    ImagePath = x.ImagePath,
                    User = x.User == null ? null : new UserSummaryDTO { Username = x.User.Username },
                    Category = x.Category == null ? null : new CategorySummaryDTO { Categoryname = x.Category.Categoryname },
                    Comments = x.Comments.Select(c => new CommentRefDTO { CommentID = c.CommentID }).ToList()
                })
                .ToList();
        }

        public PostFeedDTO? GetPostById(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be negative");
            return _context.Posts
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Comments)
                .Where(x => x.PostID == id)
                .Select(x => new PostFeedDTO
                {
                    PostID = x.PostID,
                    Title = x.Title,
                    Content = x.Content,
                    Created_at = x.Created_at,
                    Votes = x.Votes,
                    ImagePath = x.ImagePath,
                    User = x.User == null ? null : new UserSummaryDTO { Username = x.User.Username },
                    Category = x.Category == null ? null : new CategorySummaryDTO { Categoryname = x.Category.Categoryname },
                    Comments = x.Comments.Select(c => new CommentRefDTO { CommentID = c.CommentID }).ToList()
                })
                .FirstOrDefault();
        }

        public IEnumerable<CommentResponseDTO> GetPostComments(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be negative");
            return _context.Comments
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.PostID == id)
                .OrderBy(x => x.CommentCreated_at)
                .Select(x => new CommentResponseDTO
                {
                    CommentID = x.CommentID,
                    Commentcontent = x.CommentContent,
                    User = x.User == null ? null : new UserSummaryDTO { Username = x.User.Username },
                    Created_at = x.CommentCreated_at
                })
                .ToList();
        }
        //fetches
        public IEnumerable<DisplayAllUserDTO> GetAllUsers()
        {
            if (_context.Users.Count() == 0) throw new InvalidOperationException("No Users");

            return _context.Users.Select(x => new DisplayAllUserDTO
            {
                UserID = x.UserID,
                Username = x.Username,
                Role = x.Role
            });
        }
        public IEnumerable<DisplayAllPostsDTO> GetAllPosts()
        {
            if (_context.Posts.Count() == 0) throw new InvalidOperationException("No Posts");

            return _context.Posts.Select(x => new DisplayAllPostsDTO
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });
        }
        public IEnumerable<CategoryDTO> GetAllCategories()
        {
            if (_context.Categories.Count() == 0) throw new InvalidOperationException("No Categories");

            return _context.Categories.Select(x => new CategoryDTO
            {
                categoryID = x.CategoryID,
                categoryname = x.Categoryname
            }).OrderBy(x=> x.categoryID);
        }
        public IEnumerable<UsersBySearch> GetUserNamesBySearch(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("Fill the field, pretty please");
            if (!_context.Users.Any(x => x.Username.ToLower().Contains(name.ToLower()))) throw new InvalidDataException("No user with this name");

            return _context.Users.Where(x => x.Username.ToLower().Contains(name.ToLower())).Select(x => new UsersBySearch
            {
                UserID = x.UserID,
                Username = x.Username
            }).OrderBy(x => x.Username);
        }
        public IEnumerable<PostsBySearch> GetPostsBySearchAndCat(string title, string cat)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException("Fill the title field, pretty please");
            if (!_context.Posts.Any(x => x.Title.ToLower().Contains(title.ToLower()))) throw new InvalidDataException("No post with this title");
            if (string.IsNullOrWhiteSpace(cat)) throw new ArgumentNullException("Fill the cat field, pretty please");

            if (cat == "All") return _context.Posts
                .Include(x => x.User)
                .Where(x => x.Title.ToLower().Contains(title.ToLower())).Select(x => new PostsBySearch
                {
                    PostID = x.PostID,
                    UserID = x.UserID,
                    UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                    Title = x.Title,
                    Content = x.Content,
                    Created_at = x.Created_at,
                    Votes = x.Votes,
                    ImagePath = x.ImagePath
                });

            return _context.Posts
                .Include(x => x.User)
                .Where(x => x.Title.ToLower().Contains(title.ToLower()) && x.Category.Categoryname.ToLower().Contains(cat.ToLower())).Select(x => new PostsBySearch
                {
                    PostID = x.PostID,
                    UserID = x.UserID,
                    UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                    Title = x.Title,
                    Content = x.Content,
                    Created_at = x.Created_at,
                    Votes = x.Votes,
                    ImagePath = x.ImagePath
                });
        }
        public IEnumerable<PostsByCat> GetPostsByCategory(string cats)
        {
            if (string.IsNullOrWhiteSpace(cats)) throw new ArgumentNullException("Fill the field, pretty please");

            if (cats == "All") return _context.Posts.Select(x => new PostsByCat
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });

            if (!_context.Posts.Any(x => x.Category.Categoryname.ToLower().Contains(cats.ToLower()))) throw new InvalidDataException("No post with this category");

            return _context.Posts.Where(x => x.Category.Categoryname.ToLower().Contains(cats.ToLower())).Select(x => new PostsByCat
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });
        }
        public IEnumerable<GetCommentsFromPost> PostCommentsFetch(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Posts.Any(x => x.PostID == id)) throw new KeyNotFoundException($"Post not found with given ID: {id}.");

            return _context.Posts.Include(x => x.Comments).Where(x => x.PostID == id).First().Comments.Select(x => new GetCommentsFromPost
            {
                userID = x.UserID,
                username = _context.Users.Where(u => u.UserID == x.UserID).First().Username,
                commentid = x.CommentID,
                commentcontent = x.CommentContent,
                commentcreated_at = x.CommentCreated_at
            });
        }
        public IEnumerable<OwnPosts> GetOwnPosts(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Users.Include(x => x.Posts).Where(x => x.UserID == id).FirstOrDefault().Posts.Select(x => new OwnPosts
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });
        }

        public IEnumerable<OwnComments> GetOwnComments(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Comments.Where(x => x.UserID == id).Select(x => new OwnComments
            {
                CommentContent = x.CommentContent,
                CommentCreated_at = x.CommentCreated_at
            });
        }
        public IEnumerable<PostsFromOwnComment> GetPostsFromOwnComments(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Posts.Include(x => x.Comments).Where(x => x.Comments.Any(c => c.UserID == id)).Select(x => new PostsFromOwnComment
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath,
                OwnComments = x.Comments.Where(c => c.UserID == id).Select(c => new OwnComments
                {
                    CommentId = c.CommentID,
                    CommentContent = c.CommentContent,
                    CommentCreated_at = c.CommentCreated_at
                }).ToList()
            });
        }

        public IEnumerable<LikedPosts> GetLikedPosts(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Users.Include(x => x.Upvoted_Posts).Where(x => x.UserID == id).FirstOrDefault().Upvoted_Posts.Select(x => new LikedPosts
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });
        }
        public IEnumerable<DislikedPosts> GetDislikedPosts(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Users.Include(x => x.Downvoted_Posts).Where(x => x.UserID == id).FirstOrDefault().Downvoted_Posts.Select(x => new DislikedPosts
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });
        }
        public IEnumerable<Favorites> GetFavoritePosts(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Users.Include(x => x.Favourites).Where(x => x.UserID == id).FirstOrDefault().Favourites.Select(x => new Favorites
            {
                PostID = x.PostID,
                UserID = x.UserID,
                UserName = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                Title = x.Title,
                Content = x.Content,
                Created_at = x.Created_at,
                Votes = x.Votes,
                ImagePath = x.ImagePath
            });
        }

        public async Task DeleteUsers(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");

            var user = await _context.Users
                .Include(x => x.Posts)
                .Include(x => x.Favourites)
                .Include(x => x.Upvoted_Posts)
                .Include(x => x.Downvoted_Posts)
                .FirstOrDefaultAsync(x => x.UserID == id);
            if (user is null) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            using var trx = await _context.Database.BeginTransactionAsync();

            var postIds = user.Posts.Select(p => p.PostID).ToList();

            // 1. Delete all reports by this user or on their posts (no cascade configured on Report)
            var reports = await _context.Reports
                .Where(r => r.UserID == id || postIds.Contains(r.PostID))
                .ToListAsync();
            _context.Reports.RemoveRange(reports);
            await _context.SaveChangesAsync();

            // 2. Remove the user's posts from every other user's join tables
            //    (UserFavourites, UserUpvotes, UserDownvotes) — cascade won't reach these
            if (postIds.Any())
            {
                var otherUsers = await _context.Users
                    .Include(u => u.Favourites)
                    .Include(u => u.Upvoted_Posts)
                    .Include(u => u.Downvoted_Posts)
                    .Where(u => u.UserID != id && (
                        u.Favourites.Any(p => postIds.Contains(p.PostID)) ||
                        u.Upvoted_Posts.Any(p => postIds.Contains(p.PostID)) ||
                        u.Downvoted_Posts.Any(p => postIds.Contains(p.PostID))))
                    .ToListAsync();

                foreach (var other in otherUsers)
                {
                    foreach (var post in other.Favourites.Where(p => postIds.Contains(p.PostID)).ToList())
                        other.Favourites.Remove(post);
                    foreach (var post in other.Upvoted_Posts.Where(p => postIds.Contains(p.PostID)).ToList())
                        other.Upvoted_Posts.Remove(post);
                    foreach (var post in other.Downvoted_Posts.Where(p => postIds.Contains(p.PostID)).ToList())
                        other.Downvoted_Posts.Remove(post);
                }
                await _context.SaveChangesAsync();
            }

            // 3. Clear this user's own join table entries
            user.Favourites.Clear();
            user.Upvoted_Posts.Clear();
            user.Downvoted_Posts.Clear();
            await _context.SaveChangesAsync();

            // 4. Remove the user — cascade handles their Posts and Comments
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await trx.CommitAsync();
        }
        public async Task ModifyUsers(ModifyUserDTO dto)
        {
            if (dto is null) throw new ArgumentNullException("DTO nonexistant", nameof(dto));

            if (dto.id <= 0) throw new ArgumentOutOfRangeException("ID can't be 0 or negative", nameof(dto.id));

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == dto.id);
            if (user == null) throw new KeyNotFoundException("User not found");

            if (string.IsNullOrWhiteSpace(dto.name)) throw new ArgumentNullException("Username can't be empty");

            var userexists = await _context.Users.AnyAsync(x => x.Username == dto.name);
            if (userexists) throw new InvalidOperationException($"Username exists: '{dto.name}'");

            using var trx = _context.Database.BeginTransaction();
            user.Username = dto.name;
            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }
        public async Task CreatePost(PostDTO source)
        {
            if (source is null) throw new ArgumentNullException("DTO nonexistant", nameof(source));

            if (source.userID <= 0) throw new ArgumentOutOfRangeException("ID can't be 0 or negative");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == source.userID);
            if (user == null) throw new KeyNotFoundException("User not found");
            var cat = await _context.Categories.FirstOrDefaultAsync(x => x.Categoryname.ToLower() == source.categoryname.ToLower());
            if (cat == null) throw new KeyNotFoundException("Category not found");

            if (string.IsNullOrWhiteSpace(source.title)) throw new ArgumentNullException("Title can't be empty");
            if (string.IsNullOrWhiteSpace(source.content)) throw new ArgumentNullException("Content can't be empty");
            if (!string.IsNullOrEmpty(source.ImageBase64))
            {
                try
                {
                    // 1. Extract Base64 data (remove data URL prefix if present)
                    var base64Data = source.ImageBase64;
                    if (base64Data.Contains(","))
                        base64Data = base64Data.Split(',')[1];

                    // 2. Convert to bytes
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    // 3. Validate file size (max 5MB)
                    if (imageBytes.Length > 5 * 1024 * 1024)
                        throw new ArgumentException("File size exceeds 5MB limit.");

                    // 4. Get and validate extension
                    var extension = source.FileExtension ?? ".jpg";
                    if (!extension.StartsWith("."))
                        extension = "." + extension;

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowedExtensions.Contains(extension.ToLowerInvariant()))
                        throw new ArgumentException("Invalid file type. Allowed: jpg, jpeg, png, gif, webp");

                    // 5. Create uploads folder
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    // 6. Generate unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // 7. Save file to disk
                    await File.WriteAllBytesAsync(filePath, imageBytes);

                    // 8. Set the imagePath that will be saved to database
                    source.imagePath = $"/uploads/{uniqueFileName}";
                }
                catch (FormatException)
                {
                    throw new FormatException("Invalid Base64 string in ImageBase64");
                }
            }
            int beforepostcount = _context.Posts.Count();
            int beforeownpostcount = _context.Users.Include(x => x.Posts).Where(x => x.UserID == source.userID).First().Posts.Count();

            using var trx = _context.Database.BeginTransaction();
            _context.Posts.Add(new Post
            {
                UserID = source.userID,
                CategoryID = _context.Categories.Where(x => x.Categoryname.ToLower() == source.categoryname.ToLower()).First().CategoryID,
                Title = source.title,
                Content = source.content,
                Created_at = DateTime.UtcNow,
                ImagePath = source.imagePath
            });
            _context.SaveChanges();
            trx.Commit();

            int afterpostcount = _context.Posts.Count();
            int afterownpostcount = _context.Users.Include(x => x.Posts).Where(x => x.UserID == source.userID).First().Posts.Count();
            if (afterpostcount - beforepostcount != 1) throw new InvalidOperationException("Post wasn't created");
            if (afterownpostcount - beforeownpostcount != 1) throw new InvalidOperationException("Post wasn't added to own posts");


            await Task.CompletedTask;
        }
        public async Task DeletePost(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.PostID == id);
            if (post == null) throw new KeyNotFoundException("Post not found");

            int before = _context.Posts.Count();

            using var trx = _context.Database.BeginTransaction();

            _context.Posts.Remove(post);
            _context.SaveChanges();
            trx.Commit();

            int after = _context.Posts.Count();
            if (before - after != 1) throw new InvalidOperationException("Post wasn't removed");

            await Task.CompletedTask;
        }
        public async Task DeleteOwnPost(DeleteOwnPostDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == dto.userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.PostID == dto.postid);
            if (post == null) throw new KeyNotFoundException("Post not found");

            var delpost = _context.Posts.FirstOrDefault(x => x.PostID == dto.postid && x.UserID == dto.userId);

            int before = _context.Posts.Count();
            int beforeown = _context.Users.Include(x => x.Posts).Where(x => x.UserID == dto.userId).First().Posts.Count();

            using var trx = _context.Database.BeginTransaction();

            _context.Users.Include(x => x.Posts).Where(x => x.UserID == dto.userId).First().Posts.Remove(delpost);
            _context.Posts.Remove(delpost);
            _context.SaveChanges();
            trx.Commit();

            int after = _context.Posts.Count();
            int afterown = _context.Users.Include(x => x.Posts).Where(x => x.UserID == dto.userId).First().Posts.Count();
            if (before - after != 1) throw new InvalidOperationException("Post wasn't removed");
            if (beforeown - afterown != 1) throw new InvalidOperationException("Post wasn't removed from ownposts");

            await Task.CompletedTask;
        }

        public async Task FavouritePost(FavouritePostDTO favpost)
        {
            var user = await _context.Users
                .Include(x => x.Favourites)
                .FirstOrDefaultAsync(x => x.UserID == favpost.userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var post = await _context.Posts.FirstOrDefaultAsync(x => x.PostID == favpost.postId);
            if (post == null) throw new KeyNotFoundException("Post not found");

            bool alreadyFavourited = user.Favourites.Contains(post);

            using var trx = _context.Database.BeginTransaction();

            if (!alreadyFavourited)
            {
                user.Favourites.Add(post);
            }
            else
            {
                user.Favourites.Remove(post);
            }

            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }

        public async Task voteOnPost(VoteDTO dto)
        {
            var user = await _context.Users.Include(x => x.Upvoted_Posts).Include(x => x.Downvoted_Posts).FirstOrDefaultAsync(x => x.UserID == dto.userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.PostID == dto.postId);
            if (post == null) throw new KeyNotFoundException("Post not found");

            bool alreadyUpvoted = user.Upvoted_Posts.Contains(post);
            bool alreadyDownvoted = user.Downvoted_Posts.Contains(post);
            var voteDelta = 0;

            using var trx = _context.Database.BeginTransaction();

            if (dto.isUpvote) //we pressed upvote
            {
                if (alreadyUpvoted) //it was already upvoted
                {
                    user.Upvoted_Posts.Remove(post);
                    voteDelta = -1;
                }
                else
                {
                    if (alreadyDownvoted) //it was already downvoted
                    {
                        user.Downvoted_Posts.Remove(post);
                        user.Upvoted_Posts.Add(post);
                        voteDelta = 2;
                    }
                    else
                    {
                        //it was not voted
                        user.Upvoted_Posts.Add(post);
                        voteDelta = 1;
                    }
                }
            }
            else //we pressed downvote
            {
                if (alreadyDownvoted) //it was already downvoted
                {
                    user.Downvoted_Posts.Remove(post);
                    voteDelta = 1;
                }
                else
                {
                    if (alreadyUpvoted) //it was already upvoted
                    {
                        user.Upvoted_Posts.Remove(post);
                        user.Downvoted_Posts.Add(post);
                        voteDelta = -2;
                    }
                    else
                    {
                        //it was not voted
                        user.Downvoted_Posts.Add(post);
                        voteDelta = -1;
                    }
                }
            }

            post.Votes += voteDelta;

            _context.SaveChanges();
            trx.Commit();



            await Task.CompletedTask;
        }

        public async Task CommentOnPost(CommentDTO source) //could add own comment list to user later
        {
            var post = await _context.Posts.Include(x => x.Comments).FirstOrDefaultAsync(x => x.PostID == source.postID);
            if (post == null) throw new KeyNotFoundException("Post not found");
            if (string.IsNullOrWhiteSpace(source.commentcontent)) throw new ArgumentException("Comment content can't be empty");
            if (source.userID <= 0) throw new ArgumentOutOfRangeException("User ID can't be 0 or negative");
            var user = await _context.Users.Include(x => x.Upvoted_Posts).Include(x => x.Downvoted_Posts).FirstOrDefaultAsync(x => x.UserID == source.userID);
            if (user == null) throw new KeyNotFoundException("User not found");

            int before = _context.Comments.Count();

            using var trx = _context.Database.BeginTransaction();

            _context.Comments.Add(new Comment
            {
                UserID = source.userID,
                PostID = source.postID,
                CommentContent = source.commentcontent,
                CommentCreated_at = DateTime.UtcNow,
            });

            _context.SaveChanges();
            trx.Commit();

            int after = _context.Comments.Count();
            if (after - before != 1) throw new InvalidOperationException("Comment wasn't added");

            await Task.CompletedTask;
        }
        public async Task DeleteComments(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.CommentID == id);
            if (comment == null) throw new KeyNotFoundException("Comment not found");

            int before = _context.Comments.Count();

            using var trx = _context.Database.BeginTransaction();
            _context.Comments.Remove(comment);
            _context.SaveChanges();
            trx.Commit();

            int after = _context.Comments.Count();
            if (before - after != 1) throw new InvalidOperationException("Comment wasn't removed");

            await Task.CompletedTask;
        }
        public async Task CreateCategory(CategoryDTO source)
        {
            if (source is null) throw new ArgumentNullException("DTO nonexistant", nameof(source));

            if (string.IsNullOrWhiteSpace(source.categoryname)) throw new ArgumentException("Name can't be empty");

            using var trx = _context.Database.BeginTransaction();
            _context.Categories.Add(new Category
            {
                Categoryname = source.categoryname,
            });

            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }
        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
            if (category == null) throw new KeyNotFoundException("Category not found");
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be negative!");

            using var trx = _context.Database.BeginTransaction();

            var postsToDelete = _context.Posts.Where(p => p.CategoryID == id).ToList();
            _context.Posts.RemoveRange(postsToDelete);

            _context.Categories.Remove(category);

            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }
        public IEnumerable<OwnReports> GetOwnReports(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID can't be 0 or negative");
            if (!_context.Users.Any(x => x.UserID == id)) throw new KeyNotFoundException($"User not found with given ID: {id}.");

            return _context.Reports
                .Where(x => x.UserID == id)
                .Select(x => new OwnReports
                {
                    UserID = x.UserID,
                    PostID = x.PostID,
                    PostTitle = _context.Posts.Where(k => k.PostID == x.PostID).Select(k => k.Title).FirstOrDefault() ?? "(deleted post)",
                    Reason = x.ReportReason,
                    Created_at = x.ReportCreated_at,
                    ReportStatus = x.ReportStatus
                })
                .AsEnumerable()
                .Select(x =>
                {
                    x.ReportStatus = NormalizeReportStatus(x.ReportStatus);
                    return x;
                });
        }
        public IEnumerable<AdminReportDTO> GetAllReports()
        {
            return _context.Reports
                .AsNoTracking()
                .OrderByDescending(x => x.ReportCreated_at)
                .Select(x => new AdminReportDTO
                {
                    ReportID = x.ReportID,
                    PostID = x.PostID,
                    PostTitle = _context.Posts.Where(p => p.PostID == x.PostID).Select(p => p.Title).FirstOrDefault() ?? "(deleted post)",
                    ReporterUserID = x.UserID,
                    ReporterUsername = _context.Users.Where(u => u.UserID == x.UserID).Select(u => u.Username).FirstOrDefault() ?? "(deleted user)",
                    Reason = x.ReportReason,
                    Created_at = x.ReportCreated_at,
                    ReportStatus = x.ReportStatus
                })
                .AsEnumerable()
                .Select(x =>
                {
                    x.ReportStatus = NormalizeReportStatus(x.ReportStatus);
                    return x;
                })
                .ToList();
        }
        public async Task CreateReport(ReportDTO source)
        {
            if (source is null) throw new ArgumentNullException("DTO nonexistant", nameof(source));

            if (source.userID <= 0 || source.postID <= 0) throw new ArgumentOutOfRangeException("ID can't be 0 or negative");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == source.userID);
            if (user == null) throw new KeyNotFoundException("User not found");
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.PostID == source.postID);
            if (post == null) throw new KeyNotFoundException("Post not found");

            if (string.IsNullOrWhiteSpace(source.reportreason)) throw new ArgumentException("Reason can't be empty");
            var alreadyOpenReport = await _context.Reports.AnyAsync(x =>
                x.UserID == source.userID
                && x.PostID == source.postID
                && (x.ReportStatus == ReportStatusOpen || x.ReportStatus == "Pending"));
            if (alreadyOpenReport) throw new InvalidOperationException("You already have an open report for this post");

            using var trx = _context.Database.BeginTransaction();
            _context.Reports.Add(new Report
            {
                PostID = source.postID,
                UserID = source.userID,
                ReportReason = source.reportreason,
                ReportCreated_at = DateTime.UtcNow,
                ReportStatus = ReportStatusOpen
            });


            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }

        public async Task ResolveReportKeepPost(int reportId)
        {
            if (reportId <= 0) throw new ArgumentOutOfRangeException(nameof(reportId), "ID can't be 0 or negative");

            var report = await _context.Reports.FirstOrDefaultAsync(x => x.ReportID == reportId);
            if (report == null) throw new KeyNotFoundException("Report not found");
            if (!IsOpenReportStatus(report.ReportStatus)) throw new InvalidOperationException("Report is already closed");

            using var trx = _context.Database.BeginTransaction();
            report.ReportStatus = ReportStatusClosedNotDeleted;
            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }

        public async Task ResolveReportDeletePost(int reportId)
        {
            if (reportId <= 0) throw new ArgumentOutOfRangeException(nameof(reportId), "ID can't be 0 or negative");

            var report = await _context.Reports.FirstOrDefaultAsync(x => x.ReportID == reportId);
            if (report == null) throw new KeyNotFoundException("Report not found");
            if (!IsOpenReportStatus(report.ReportStatus)) throw new InvalidOperationException("Report is already closed");

            using var trx = _context.Database.BeginTransaction();

            var targetPostId = report.PostID;
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.PostID == targetPostId);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            var openReportsOnPost = await _context.Reports
                .Where(x => x.PostID == targetPostId && (x.ReportStatus == ReportStatusOpen || x.ReportStatus == "Pending"))
                .ToListAsync();
            foreach (var openReport in openReportsOnPost)
            {
                openReport.ReportStatus = ReportStatusClosedDeleted;
            }

            _context.SaveChanges();
            trx.Commit();

            await Task.CompletedTask;
        }
    }
}

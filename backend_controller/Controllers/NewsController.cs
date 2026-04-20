using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizsgaController.Dtos;
using vizsgaController.Model;

namespace vizsgaController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {

        private readonly INewsModel _model;
        public NewsController(INewsModel model)
        {
            _model = model;
        }
        [HttpGet]
        public ActionResult<IEnumerable<PostFeedDTO>> GetFeed([FromQuery] int page = 0, [FromQuery] int pageSize = 10, [FromQuery] string? category = null)
        {
            try
            {
                return Ok(_model.GetFeed(page, pageSize, category));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("{id:int}")]
        public ActionResult<PostFeedDTO> GetPostById(int id)
        {
            var post = _model.GetPostById(id);
            if (post == null) return NotFound();
            return Ok(post);
        }
        [HttpGet("{id:int}/comments")]
        public ActionResult<IEnumerable<CommentResponseDTO>> GetPostComments(int id)
        {
            return Ok(_model.GetPostComments(id));
        }
        [HttpGet("getallusers")]
        public async Task<ActionResult<IEnumerable<DisplayAllUserDTO>>> GetAllUsers()
        {
            try
            {
                return Ok(_model.GetAllUsers());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("getallposts")]
        public async Task<ActionResult<IEnumerable<DisplayAllPostsDTO>>> GetAllPosts()
        {
            try
            {
                return Ok(_model.GetAllPosts());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("getallcats")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCats()
        {
            try
            {
                return Ok(_model.GetAllCategories());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("search_user")]
        public async Task<ActionResult<IEnumerable<UsersBySearch>>> GetUserNameBySearch([FromQuery] string name)
        {
            try
            {
                var users = _model.GetUserNamesBySearch(name);
                return Ok(users);
            }
            catch (InvalidDataException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("search_post")]
        public async Task<ActionResult<IEnumerable<PostsBySearch>>> GetPostBySearchAndCat([FromQuery] string title, [FromQuery] string cat)
        {
            try
            {
                var posts = _model.GetPostsBySearchAndCat(title, cat);
                return Ok(posts);
            }
            catch (InvalidDataException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("search_post_by_cat")]
        public async Task<ActionResult<IEnumerable<PostsByCat>>> GetPostByCat([FromQuery] string cat)
        {
            try
            {
                var posts = _model.GetPostsByCategory(cat);
                return Ok(posts);
            }
            catch (InvalidDataException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("getcommentsfrompost")]
        public async Task<ActionResult<IEnumerable<GetCommentsFromPost>>> FetchPostComments([FromQuery] int id)
        {
            try
            {
                var posts = _model.PostCommentsFetch(id);
                return Ok(posts);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("getownposts")]
        public async Task<ActionResult<IEnumerable<OwnPosts>>> GetOwnPosts([FromQuery] int id)
        {
            try
            {
                var posts = _model.GetOwnPosts(id);
                return Ok(posts);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("getowncomments")]
        public async Task<ActionResult<IEnumerable<OwnComments>>> GetOwnComments([FromQuery] int id)
        {
            try
            {
                var comments = _model.GetOwnComments(id);
                return Ok(comments);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("postsfromowncomment")]
        public async Task<ActionResult<IEnumerable<PostsFromOwnComment>>> GetPostsFromOwnComments([FromQuery] int id)
        {
            try
            {
                var comments = _model.GetPostsFromOwnComments(id);
                return Ok(comments);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("getlikedposts")]
        public async Task<ActionResult<IEnumerable<LikedPosts>>> GetLikedPosts([FromQuery] int id)
        {
            try
            {
                var liked = _model.GetLikedPosts(id);
                return Ok(liked);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("getdislikedposts")]
        public async Task<ActionResult<IEnumerable<DislikedPosts>>> GetDislikedPosts([FromQuery] int id)
        {
            try
            {
                var disliked = _model.GetDislikedPosts(id);
                return Ok(disliked);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpGet("getfavorites")]
        public async Task<ActionResult<IEnumerable<Favorites>>> GetFavorites([FromQuery] int id)
        {
            try
            {
                var favs = _model.GetFavoritePosts(id);
                return Ok(favs);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_users")]
        public async Task<ActionResult> DeleteUsers([FromQuery] int id)
        {
            try
            {
                await _model.DeleteUsers(id);
                return Ok("User managed successfully");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest($"Hiba történt: {e.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("modify_user")]
        public async Task<ActionResult> ModifyUser([FromBody]ModifyUserDTO userDto)
        {
            try
            {
                await _model.ModifyUsers(userDto);
                return Ok("User modified successfully");
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [Authorize]
        [HttpPost("create_posts")]
        public async Task<ActionResult> CreatePost([FromBody] PostDTO source)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Unauthorized();
                source.userID = userId;
                await _model.CreatePost(source);
                return Ok("Post created successfully");
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_posts")]
        public async Task<ActionResult> DeletePosts([FromQuery] int id)
        {
            try
            {
                await _model.DeletePost(id);
                return Ok("Post deleted successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [Authorize]
        [HttpDelete("delete_own_post")]
        public async Task<ActionResult> DeleteOwnPost([FromQuery] DeleteOwnPostDTO dto)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Unauthorized();
                await _model.DeleteOwnPost(new DeleteOwnPostDTO { postid = dto.postid, userId = dto.userId });
                return Ok("Your post has been deleted");    
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [Authorize]
        [HttpPost("favourite_posts")]
        public async Task<ActionResult> FavouritePosts([FromBody] FavouritePostDTO dto)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Unauthorized();
                dto.userId = userId;
                await _model.FavouritePost(dto);
                return Ok("Post favourited/unfavourited successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize]
        [HttpPost("vote")]
        public async Task<ActionResult> Vote([FromBody] VoteDTO dto)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Unauthorized();
                await _model.voteOnPost(new VoteDTO { userId = userId, postId = dto.postId, isUpvote = dto.isUpvote });
                return Ok("Voted/Unvoted");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize]
        [HttpPost("comment")]
        public async Task<ActionResult> Comment([FromBody] CommentDTO source)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Unauthorized();
                await _model.CommentOnPost(new CommentDTO
                {
                    userID = userId,
                    postID = source.postID,
                    commentcontent = source.commentcontent
                });
                return Ok("Comment posted");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_comments")]
        public async Task<ActionResult> DeleteSelectedComment([FromQuery] int id)
        {
            try
            {
                await _model.DeleteComments(id);
                return Ok("Comment deleted successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create_category")]
        public async Task<ActionResult> CreateCat([FromBody] CategoryDTO source)
        {
            try
            {
                await _model.CreateCategory(source);
                return Ok("Category created successfully");
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_category")]
        public async Task<ActionResult> DeleteSelectedCategory([FromQuery] int id)
        {
            try
            {
                await _model.DeleteCategory(id);
                return Ok("Category deleted successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [HttpGet("ownreports")]
        public async Task<ActionResult<IEnumerable<OwnReports>>> OwnReports([FromQuery] int id)
        {
            try
            {
                var reps = _model.GetOwnReports(id);
                return Ok(reps);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("getallreports")]
        public async Task<ActionResult<IEnumerable<AdminReportDTO>>> GetAllReports()
        {
            try
            {
                return Ok(_model.GetAllReports());
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("resolve_report_keep")]
        public async Task<ActionResult> ResolveReportKeep([FromQuery] int id)
        {
            try
            {
                await _model.ResolveReportKeepPost(id);
                return Ok("Report closed without deleting post");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("resolve_report_delete")]
        public async Task<ActionResult> ResolveReportDelete([FromQuery] int id)
        {
            try
            {
                await _model.ResolveReportDeletePost(id);
                return Ok("Report closed and post deleted");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }

        [Authorize]
        [HttpPost("create_report")]
        public async Task<ActionResult> CreateRep([FromBody] ReportDTO source)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Unauthorized();
                source.userID = userId;
                await _model.CreateReport(source);
                return Ok("Report submitted successfully");
            }
            catch (ArgumentNullException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Hiba történt");
            }
        }
    }
}

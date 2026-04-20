using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizsgaController.Dtos;
using vizsgaController.Model;

namespace vizsgaController.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserModel _model;
        public UserController(IUserModel model)
        {
            _model = model;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDataDTO>> Me()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            return Ok(new UserDataDTO
            {
                ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                UserName = User.FindFirstValue(ClaimTypes.Name)!,
                Role = User.FindFirstValue(ClaimTypes.Role) ?? "User"
            });
        }

        [HttpPost("registration")]
        public async Task<ActionResult> Registration([FromBody] RegistrationDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto?.username) || string.IsNullOrWhiteSpace(dto?.email) || string.IsNullOrWhiteSpace(dto?.password))
                {
                    return UnprocessableEntity(new { message = "Username, email and password are required." });
                }
                await _model.Registration(dto.username, dto.email, dto.password);
                return Ok(new { message = "Registration successful" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // [HttpPost("trackusage")]
        // public async Task<ActionResult> TrackUsage([FromQuery] string username, [FromQuery] int amount)
        // {
        //     try
        //     {
        //         await _model.TrackUsage(username, amount);
        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        // [HttpPost("getusage")]
        // public async Task<ActionResult<int>> GetUsage([FromQuery] string username)
        // {
        //     try
        //     {
        //         return Ok(_model.GetUsage(username));
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        // [Authorize(Roles = "Admin")]
        // [HttpPost("setusage")]
        // public async Task<ActionResult> SetUsage([FromQuery] string username, [FromQuery] int amount)
        // {
        //     try
        //     {
        //         await _model.SetUsage(username, amount);
        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto? dto)
        {
            if (dto == null) return UnprocessableEntity("Username and password are required.");
            var username = dto.username?.Trim();
            var password = dto.password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return UnprocessableEntity("Username and password are required.");
            }
            var user = _model.ValidateUser(username, password);
            if (user == null) return Unauthorized();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            return Ok(new UserDataDTO { ID = user.UserID, UserName = user.Username, Role = user.Role });
        }
        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("rolemodify")]
        public async Task<ActionResult> RoleModify([FromQuery] int userid)
        {
            try
            {
                await _model.RoleModify(userid);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [Authorize]
        [HttpPut("modifypassword")]
        public async Task<ActionResult> ModifyPassword([FromBody] ModifyPasswordDTO dto)
        {
            try
            {
                var username = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized();
                await _model.ModifyPassword(username, dto.newPassword, dto.currentPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("allusers")]

        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            try
            {
                return Ok(_model.GetUsers());
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }
    }
}

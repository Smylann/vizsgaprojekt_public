using System.Security.Cryptography;
using System.Text;
using vizsgaController.Dtos;
using vizsgaController.Persistence;

namespace vizsgaController.Model
{
    public class UserModel : IUserModel
    {
        private readonly NewsDbContext _context;
        public UserModel(NewsDbContext context)
        {
            _context = context;
        }
        public async Task Registration(string name, string email, string password)
        {
            if (_context.Users.Any(u => u.Username == name))
            {
                throw new InvalidOperationException("Already exists");
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Name, password or email empty");
            }

            using var trx = _context.Database.BeginTransaction();
            {
                _context.Users.Add(new User { Username = name, Useremail = email, Userpassword = HashPassword(password), Role = "User" });
                _context.SaveChanges();
                trx.Commit();
            }
        }

        public string salt = "reddit2";
        public User ValidateUser(string username, string password)
        {
            if (username == string.Empty || password == string.Empty)
            {
                throw new ArgumentException("Username or password is null");
            }
            var hash = HashPassword(password);
            var user = _context.Users.Where(x => x.Username == username);
            return user.Where(x => x.Userpassword == hash).FirstOrDefault();
        }
        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        public async Task RoleModify(int userid)
        {
            if (userid < 0)
            {
                throw new ArgumentException("UserID is invalid");
            }
            using var trx = _context.Database.BeginTransaction();
            {
                var user = _context.Users.Where(x => x.UserID == userid).FirstOrDefault();
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }
                if (user.Role == "User")
                {
                    user.Role = "Admin";
                }
                else
                {
                    user.Role = "User";
                }
                _context.SaveChanges();
                trx.Commit();
            }
        }
        public async Task ModifyPassword(string username, string password, string currentpass)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentException();
            using var trx = _context.Database.BeginTransaction();
            {
                var user = _context.Users.Where(x => x.Username == username).FirstOrDefault();
                if (user.Userpassword != HashPassword(currentpass))
                {
                    throw new InvalidOperationException("Current password is incorrect");
                }
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }
                user.Userpassword = HashPassword(password);
                _context.SaveChanges();
                trx.Commit();
            }
        }

        /*
        public async Task TrackUsage(string username, int amount)
        {
            using var trx = _context.Database.BeginTransaction();
            {
                var user = _context.Users.Where(x => x.Username == username).FirstOrDefault();
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }
                user.Usage += amount;
                _context.SaveChanges();
                trx.Commit();
            }
        }

        public async Task GetUsage(string username)
        {
            await Task.FromResult(_context.Users.Where(x => x.Username == username).Select(x => x.Usage).FirstOrDefault());
        }

        public async Task SetUsage(string username, int amount)
        {
            using var trx = _context.Database.BeginTransaction();
            {
                var user = _context.Users.Where(x => x.Username == username).FirstOrDefault();
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }
                user.Usage = amount;
                _context.SaveChanges();
                trx.Commit();
            }
        }
        */


        public IEnumerable<UserDTO> GetUsers()
        {
            return _context.Users.Select(x => new UserDTO
            {
                userID = x.UserID,
                username = x.Username,
                role = x.Role
            }).OrderBy(x => x.userID);
        }
    }
}

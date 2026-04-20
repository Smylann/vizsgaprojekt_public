using vizsgaController.Model;
using vizsgaController.Persistence;

namespace ControllerTesting
{
    public class UserModelTest
    {
        private readonly UserModel _model;
        private readonly NewsDbContext _context;

        public UserModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new UserModel(_context);
        }

        [Fact]
        public async Task Register_Valid()
        {
            int beforeCount = _context.Users.Count();
            await _model.Registration("testuser", "testuser@example.com", "testpassword");
            int afterCount = _context.Users.Count();
            Assert.Equal(beforeCount + 1, afterCount);
        }

        [Fact]
        public async Task Register_UserAlreadyExist()
        {
            await _model.Registration("testuser", "testuser@example.com", "testpassword");
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _model.Registration("testuser", "testuser@example.com", "testpassword");
            });
        }

        [Fact]
        public async Task Register_EmptyData()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _model.Registration("", "", "");
            });
        }

        //////////////////////////////////////////

        [Fact]
        public void ValidateUser_Valid()
        {
            _model.Registration("validuser", "validuser@example.com", "validpassword").Wait();

            var result = _model.ValidateUser("validuser", "validpassword");

            Assert.NotNull(result);
            Assert.Equal("validuser", result.Username);
            Assert.Equal("validuser@example.com", result.Useremail);
        }

        [Fact]
        public void ValidateUser_UserOrPassEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                {
                    _model.ValidateUser("", "");
                });
        }

        //////////////////////////////////////////

        [Fact]
        public async Task RoleModify_Valid()
        {
            await _model.Registration("roleuser", "asd@asd.com", "password");
            var user = _context.Users.Where(x => x.Username == "roleuser").FirstOrDefault();
            await _model.RoleModify(user.UserID);
            Assert.Equal(user.Role, "Admin");
            await _model.RoleModify(user.UserID);
            Assert.Equal(user.Role, "User");
        }

        [Fact]
        public async Task RoleModify_UserIDInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _model.RoleModify(-1);
            });
        }

        //////////////////////////////////////////
        [Fact]
        public async Task ChangePass_valid()
        {
            await _model.Registration("changepassuser", "asd@asd.com", "password");
            var userpasshashold = _context.Users.Where(x => x.Username == "changepassuser").FirstOrDefault().Userpassword;
            await _model.ModifyPassword("changepassuser", "newpassword", "password");
            var userpasshashnew = _context.Users.Where(x => x.Username == "changepassuser").FirstOrDefault().Userpassword;
            Assert.NotEqual(userpasshashold, userpasshashnew);
        }

        [Fact]
        public async Task ChangePass_InvalidOldPass()
        {
            await _model.Registration("changepassuser2", "asd2@asd.com", "password");
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _model.ModifyPassword("changepassuser2", "newpassword", "wrongpassword");
            });
        }

        [Fact]
        public async Task ChangePass_UserNotExist()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _model.ModifyPassword("nonexistentuser", "newpassword", "password");
            });
        }
    }
}
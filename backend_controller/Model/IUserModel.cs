using vizsgaController.Dtos;
using vizsgaController.Persistence;

namespace vizsgaController.Model
{
    public interface IUserModel
    {
        public Task Registration(string name, string email, string password);
        public User ValidateUser(string username, string password);
        public Task RoleModify(int userid);
        // public Task TrackUsage(string username, int amount);
        // public Task GetUsage(string username);
        // public Task SetUsage(string username, int amount);
        public Task ModifyPassword(string username, string password, string currentpass);
        public IEnumerable<UserDTO> GetUsers();
    }
}

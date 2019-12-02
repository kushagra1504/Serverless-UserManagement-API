using ServerLess_Zip.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerLess_Zip.Services
{
    public interface IUserService
    {
         Task<User> GetUserByEmail(string email);

        Task<List<User>> GetAllUsers();

        Task CreateUser(User user);

    }
}

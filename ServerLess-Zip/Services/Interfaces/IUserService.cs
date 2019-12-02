using ServerLess_Zip.Model;
using System.Threading.Tasks;

namespace ServerLess_Zip.Services
{
    public interface IUserService
    {
         Task<User> GetUserByEmail(string email);
    }
}

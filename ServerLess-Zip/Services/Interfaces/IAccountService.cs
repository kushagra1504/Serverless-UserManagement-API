using ServerLess_Zip.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerLess_Zip.Services
{
    public interface IAccountService
    {
         Task<Account> GetAccountByEmail(string email);

        Task<List<Account>> GetAllAccounts();

        Task CreateAccount(AccountRequest accountRequest, User user);

    }
}

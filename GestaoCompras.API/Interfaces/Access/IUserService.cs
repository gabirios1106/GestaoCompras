using GestaoCompras.Models.Access;
using Microsoft.AspNetCore.Identity;

namespace GestaoCompras.API.Interfaces.Access
{
    public interface IUserService
    {
        Task<dynamic> FindAsync<T>(string userName);

        Task<IdentityResult> CreateAsync(User user);

        Task<IdentityResult> DeleteAsync(User user);

        Task<bool> ExistsByEmailAsync(string email);
    }
}

using GestaoCompras.Models.Access;

namespace GestaoCompras.API.Interfaces.Access
{
    public interface IApplicationUserService
    {
        Task<dynamic> GetApplicationUserByAppIdAsync<T>(Guid appId, bool showInactive = false);

        Task<bool> VerifyHashedPassword(string authorization);

        Task CreateAsync(ApplicationUser applicationUser);

        Task UpdateAsync(ApplicationUser applicationUser);

        Task DeleteAsync(ApplicationUser applicationUser);

        Task<bool> ExistsByNameAsync(string appName);
    }
}

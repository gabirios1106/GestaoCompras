using GestaoCompras.Models.Users;

namespace GestaoCompras.API.Interfaces.Users
{
    public interface IUserDataService
    {
        Task<dynamic> FindByUserIdAsync<T>(Guid userId, bool showInactive = false);

        Task<UserData> GetUserDataById(int userDataId);

        Task<int> GetUserDataIdAsync(Guid userId);

        Task InsertAsync(UserData userData);

        Task UpdateAsync(UserData userData);
    }
}

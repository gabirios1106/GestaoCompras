using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Users;
using GestaoCompras.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace GestaoCompras.API.Services.Users
{
    public class UserDataService : IUserDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserDataService> _logger;

        public UserDataService(ApplicationDbContext context, ILogger<UserDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<dynamic> FindByUserIdAsync<T>(Guid userId, bool showInactive = false)
        {
            var userData = await _context.UserData
                                         .AsNoTracking()
                                         .Include(ud => ud.User).AsSplitQuery()
                                         .Where(u => u.UserId == userId)
                                         .FirstOrDefaultAsync();

            if (userData == null)
                throw new NotFoundException($"Não existe um usuário com o Id {userId}");
            else
            {
                if (typeof(T).IsAssignableFrom(typeof(UserData)))
                    return userData;
                else if (typeof(T).IsAssignableFrom(typeof(UserDataGetDTO)))
                {
                    var userDataGetDTO = new UserDataGetDTO(userData.Id, userData.Name, userData.UserId);
                    return await Task.FromResult(userDataGetDTO);
                }
                else
                    throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
            }
        }

        public async Task<UserData> GetUserDataById(int userDataId)
        {
            var userData = await _context.UserData.AsNoTracking().Where(u => u.Id == userDataId).FirstOrDefaultAsync();

            if (userData == null)
                throw new NotFoundException($"Não existem dados de usuário com o Id {userDataId}");
            else
                return userData;
        }

        public async Task<int> GetUserDataIdAsync(Guid userId) => await _context.UserData.AsNoTracking().Where(ud => ud.UserId == userId).Select(un => un.Id).FirstAsync();

        public async Task InsertAsync(UserData userData)
        {
            try
            {
                _context.UserData.Add(userData);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Falha na registrar dados do usuário {UserId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", userData.UserId, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateAsync(UserData userData)
        {
            try
            {
                _context.Entry(userData).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogInformation("Erro ao atualizar dados do usuário. Message: {Message}", e.Message);
                throw new DbUpdateConcurrencyException(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Erro ao atualizar dados do usuário. Message: {Message}", e.Message);
                throw new Exception(e.Message);
            }
        }
    }

}

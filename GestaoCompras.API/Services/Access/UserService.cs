using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Access;
using GestaoCompras.Models.Access;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaoCompras.API.Services.Access
{
    public class UserService(ApplicationDbContext context, UserManager<User> userManager, ILogger<UserService> logger) : IUserService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<dynamic> FindAsync<T>(string userName)
        {
            var queryable = _context.Users.AsNoTracking().AsSplitQuery().Where(u => u.UserName == userName);
            var user = await queryable.FirstOrDefaultAsync();

            if (user == null)
                throw new NotFoundException($"O usuário {userName} não existe");
            else
            {
                if (typeof(T).IsAssignableFrom(typeof(User)))
                    return user;
                else if (typeof(T).IsAssignableFrom(typeof(UserGetDTO)))
                {
                    var userGetDTO = new UserGetDTO(user.Id, user.Email);
                    return await Task.FromResult(userGetDTO);
                }
                else
                    throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
            }
        }

        public async Task<IdentityResult> CreateAsync(User user)
        {
            try
            {
                var identityResult = await _userManager.CreateAsync(user, user.PasswordHash);
                return identityResult;
            }
            catch (Exception e)
            {
                _logger.LogError("Falha ao tentar registrar usuário. UserName: {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", user.Email, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user)
        {
            try
            {
                var userDelete = await _userManager.FindByEmailAsync(user.Email);

                if (userDelete != null)
                    return await _userManager.DeleteAsync(userDelete);
                else
                    return new IdentityResult();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError("Falha ao tentar excluir usuário. UserName: {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", user.Email, e.Message, e.StackTrace);
                throw new DbForeignKeyException(e.Source, e.InnerException.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Falha ao tentar excluir usuário. UserName: {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", user.Email, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.AsNoTracking().AnyAsync(u => u.Email == email);
            }
            catch (Exception e)
            {
                _logger.LogError("Falha na busca de usuário por e-mail. Email: {Email}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", email, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }
    }

}

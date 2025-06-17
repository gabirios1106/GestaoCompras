using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Access;
using GestaoCompras.Enums.General;
using GestaoCompras.Models.Access;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace GestaoCompras.API.Services.Access
{
    public class ApplicationUserService(ApplicationDbContext context, ILogger<ApplicationUserService> logger) : IApplicationUserService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ApplicationUserService> _logger = logger;

        public async Task<dynamic> GetApplicationUserByAppIdAsync<T>(Guid appId, bool showInactive = false)
        {
            var queryable = _context.ApplicationUser
                                    .AsNoTracking()
                                    .Where(b => b.AppId == appId);

            if (!showInactive)
                queryable = queryable.Where(c => c.Status == Status.ATIVO);

            var appUser = await queryable.FirstOrDefaultAsync();

            if (appUser == null)
                throw new NotFoundException($"O usuário {appId} não existe");
            else
            {
                if (typeof(T).IsAssignableFrom(typeof(ApplicationUser)))
                    return appUser;
                else if (typeof(T).IsAssignableFrom(typeof(ApplicationUserGetDTO)))
                {
                    var applicationUserGetDTO = new ApplicationUserGetDTO(appUser.AppId, appUser.AppName, appUser.CreatedAt, appUser.Status);
                    return Task.FromResult(applicationUserGetDTO);
                }
                else
                    throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
            }
        }

        public async Task<bool> VerifyHashedPassword(string authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(authorization))
                    return false;

                if (!authorization.Contains("Basic"))
                    return false;

                var secret = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authorization.Replace("Basic ", string.Empty)));

                if (!secret.Contains(':'))
                    return false;

                var clientCredential = secret.Split(':');

                if (clientCredential.Length != 2)
                    return false;

                var client_id = Guid.TryParse(clientCredential[0], out Guid client_id_guid) ? client_id_guid : Guid.Empty;
                var client_secret = clientCredential[1];

                var applicationUser = await _context.ApplicationUser.FirstOrDefaultAsync(au => au.AppId == client_id);

                if (applicationUser == null)
                    return false;

                return BCrypt.Net.BCrypt.EnhancedVerify(client_secret, applicationUser.AppPasswordHash);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao verificar de senha do usuário de aplicação. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task CreateAsync(ApplicationUser applicationUser)
        {
            try
            {
                applicationUser.AppPasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(applicationUser.AppPasswordHash, 13);

                _context.ApplicationUser.Add(applicationUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao cadastrar o usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", applicationUser.AppName, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateAsync(ApplicationUser applicationUser)
        {
            try
            {
                applicationUser.AppPasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(applicationUser.AppPasswordHash, 13);

                _context.Entry(applicationUser).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError("Erro ao alterar a senha do usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", applicationUser.AppName, e.Message, e.StackTrace);
                throw new DbConcurrencyException(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao alterar a senha do usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", applicationUser.AppName, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task DeleteAsync(ApplicationUser applicationUser)
        {
            try
            {
                _context.ApplicationUser.Remove(applicationUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError("Erro ao excluir o usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", applicationUser.AppName, e.Message, e.StackTrace);
                throw new DbUpdateException(e.Message, e);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao excluir o usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", applicationUser.AppName, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> ExistsByNameAsync(string appName)
        {
            try
            {
                return await _context.ApplicationUser.AnyAsync(au => au.AppName == appName);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao verificar a existência do usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appName, e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }
    }
}

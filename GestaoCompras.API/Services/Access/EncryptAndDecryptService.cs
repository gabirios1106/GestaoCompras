using GestaoCompras.API.Interfaces.Access;
using System.Security.Cryptography;
using System.Text;

namespace GestaoCompras.API.Services.Access;

public class EncryptAndDecryptService : IEncryptAndDecryptService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptAndDecryptService> _logger;

    private static string _encriptionKey;

    public EncryptAndDecryptService(IConfiguration configuration, ILogger<EncryptAndDecryptService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _encriptionKey = _configuration.GetValue<string>("EncryptConfiguration:EncryptionKey");
    }

    public string Encrypt(string text)
    {
        try
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_encriptionKey);
            aesAlg.IV = new byte[16];

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using var swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(text);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao encriptar texto. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            throw new Exception(e.Message);
        }
    }

    public string Decrypt(string encryptedText)
    {
        try
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_encriptionKey);
            aesAlg.IV = new byte[16];

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao decriptar texto. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            throw new Exception(e.Message);
        }
    }
}

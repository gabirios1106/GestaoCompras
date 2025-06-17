namespace GestaoCompras.API.Interfaces.Access
{
    public interface IEncryptAndDecryptService
    {
        string Encrypt(string text);

        string Decrypt(string encryptedText);
    }
}

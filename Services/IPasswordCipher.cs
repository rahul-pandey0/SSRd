namespace SSRd.Services;

public interface IPasswordCipher
{
    string Encrypt(string plainText, string key);
    string Decrypt(string cipherTextBase64, string key);
}

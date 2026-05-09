using System.Security.Cryptography;
using System.Text;

namespace SSRd.Services;

// Ports the legacy `Encrypt` class (DES) so values match the existing tb_user_password.s_Password.
// Legacy code passed a 35-byte IV array; .NET DES only consumes the first 8 bytes.
public class LegacyDesPasswordCipher : IPasswordCipher
{
    private static readonly byte[] LegacyIv =
    {
        18, 52, 86, 120, 144, 171, 205, 218, 239, 255,
        17, 34, 35, 64, 171, 43, 60, 241, 90, 112,
        139, 16, 32, 144, 161, 179, 198, 5, 119, 226,
        213, 106, 122, 136, 238
    };

    public string Encrypt(string plainText, string key)
    {
        var (byKey, iv) = DeriveKeyAndIv(key);
        using var des = DES.Create();
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.PKCS7;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateEncryptor(byKey, iv), CryptoStreamMode.Write))
        {
            var input = Encoding.UTF8.GetBytes(plainText);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherTextBase64, string key)
    {
        var (byKey, iv) = DeriveKeyAndIv(key);
        using var des = DES.Create();
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.PKCS7;

        var input = Convert.FromBase64String(cipherTextBase64);
        using var ms = new MemoryStream(input);
        using var cs = new CryptoStream(ms, des.CreateDecryptor(byKey, iv), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    private static (byte[] Key, byte[] Iv) DeriveKeyAndIv(string key)
    {
        var trimmed = key.Trim();
        var len = Math.Min(trimmed.Length, 8);
        var byKey = Encoding.UTF8.GetBytes(trimmed.Substring(0, len));
        var iv = LegacyIv.Take(8).ToArray();
        return (byKey, iv);
    }
}

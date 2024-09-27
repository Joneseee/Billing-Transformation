using System.Security.Cryptography;

namespace IntegrationLogic.Helpers;

public static class CryptoHelper
{
    #region Properties

    internal static byte[] Key128 = {
        141, 123, 142, 234, 231, 13, 94, 101, 123, 12, 55, 98, 23, 91, 4, 111, 31, 70, 21, 42, 1, 52, 67, 82, 95,
        129, 187, 162, 6, 0, 12, 32
    };

    internal static byte[] Iv128 = { 98, 45, 76, 88, 214, 222, 200, 109, 2, 234, 12, 52, 44, 53, 23, 78 };

    #endregion

    #region Methods

    public static string Encrypt(string sPlainText)
    {
        if (string.IsNullOrEmpty(sPlainText)) return string.Empty;
        using Aes aes = Aes.Create();
        aes.Key = Key128;
        aes.IV = Iv128;
        byte[] array;
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream memoryStream = new())
        {
            using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(sPlainText);
                }
                array = memoryStream.ToArray();
            }
        }
        return Convert.ToBase64String(array);
    }

    public static string Decrypt(string sCipherText)
    {
        if (string.IsNullOrEmpty(sCipherText)) return string.Empty;
        using Aes aes = Aes.Create();
        aes.Key = Key128;
        aes.IV = Iv128;
        byte[] buffer = Convert.FromBase64String(sCipherText);
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }

    #endregion
}
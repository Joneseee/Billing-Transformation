using System.Security.Cryptography;
using System.Text;

namespace IntegrationLogic;

public class SiteIdGenerator
{
    public static ulong Generate10DigitNumber(int iAccountId, int iContactId)
    {
        // Combine the two numbers into a string
        string combinedId = $"{iAccountId}{iContactId}";

        // Use SHA-256 to generate a hash of the combined ID
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedId));

        // Convert the first 8 bytes of the hash to a ulong
        ulong hashInt = BitConverter.ToUInt64(hashBytes, 0);

        // Ensure the result is a 10-digit number by taking modulo 10^10
        ulong siteId = hashInt % 10000000000UL;

        return siteId;
    }
}
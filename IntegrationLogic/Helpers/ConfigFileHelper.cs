using System.Text.Json;
using IntegrationLogic.Models;

namespace IntegrationLogic.Helpers;

public class ConfigFileHelper
{
    #region Properties

    // Path to the config.json file
    private static readonly string PathToConfig = AppDomain.CurrentDomain.BaseDirectory + "config.json";

    #endregion

    #region Methods

    // Read config.json file
    public static Config ReadFile()
    {
        Config oConfig = new()
        {
            Url = string.Empty,
            DatabaseName = string.Empty,
            AuthenticationKey = string.Empty,
            Usercode = string.Empty,
            FilePath = string.Empty,
            LogPath = string.Empty,
            SqlServer = string.Empty,
            SqlDatabase = string.Empty,
            SqlUsername = string.Empty,
            SqlPassword = string.Empty
        };

        if (File.Exists(PathToConfig))
        {
            // Decrypt config.json file and deserialize
            var decrypted = CryptoHelper.Decrypt(File.ReadAllText(PathToConfig));
            oConfig = JsonSerializer.Deserialize<Config>(decrypted)!;
        }
        else
        {
            // Create config.json in application folder with encrypted serialized Config
            File.WriteAllText(PathToConfig, CryptoHelper.Encrypt(JsonSerializer.Serialize(oConfig)));
        }
        return oConfig;
    }

    // Save config.json file
    public static void SaveFile(Config oConfig)
    {
        // Create config.json in application folder with encrypted serialized Config
        File.WriteAllText(PathToConfig, CryptoHelper.Encrypt(JsonSerializer.Serialize(oConfig)));
    }

    #endregion
}
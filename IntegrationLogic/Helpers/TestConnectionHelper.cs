namespace IntegrationLogic.Helpers;

public class TestConnectionHelper
{
    // Test Sdk Connection
    public static async Task<bool> SdkConnection(string sUrl, string sDatabaseName, string sAuthenticationKey)
    {
        bool bSuccess = false;
        try
        {
            SdkHelper sdkHelper = new SdkHelper(sUrl, sDatabaseName, sAuthenticationKey);
            bSuccess = await sdkHelper.TestSdk();
            sdkHelper.Dispose();
        }
        catch
        {
            // ignored
        }
        return bSuccess;
    }

    // Test Bde Connection
    public static bool BdeConnection(string sSqlServer, string sSqlDatabase, string sSqlUsername, string sSqlPassword)
    {
        try
        {
            BdeHelper bdeHelper = new BdeHelper(sSqlServer, sSqlDatabase, sSqlUsername, sSqlPassword);
            return bdeHelper.IsServerConnected();
        }
        catch
        {
            // ignored
        }
        return false;
    }
}
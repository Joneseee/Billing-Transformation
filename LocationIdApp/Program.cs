using System.Windows;
using IntegrationLogic;
using IntegrationLogic.Helpers;
using IntegrationLogic.Models;
using schemas.appliedsystems.com.epic.sdk._2017._02._account;

namespace LocationIdApp;

class Program
{
    private static void Main(string[] args)
    {
        CommandLineArgs arguments = ArgumentsHelper.GetCommandLineArgs();

        if (arguments.ContactId == null)
        {
            MessageBox.Show(
                $"Location ID Generator can only be launched from the Contact View.",
                "Error", MessageBoxButton.OK,
                MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
        }
        else
        {
            try
            {
                string user = string.Empty;

                if (arguments.User != null)
                {
                    user = arguments.User;
                }

                Config config = ConfigFileHelper.ReadFile();

                if (config is { DatabaseName: not null, AuthenticationKey: not null, Url: not null })
                {
                    SdkHelper sdkHelper = new(config.Url, config.DatabaseName, config.AuthenticationKey, user);

                    Contact contact = sdkHelper.GetContact(Convert.ToInt32(arguments.ContactId)).GetAwaiter().GetResult();
                    string siteId = $"{SiteIdGenerator.Generate10DigitNumber(contact.AccountID, contact.ContactID)}";


                    bool updated = false;
                    bool notNull = false;
                    string sdkException = string.Empty;

                    try
                    {
                        if (string.IsNullOrEmpty(contact.SiteID))
                        {
                            contact.SiteID = siteId;
                            sdkHelper.UpdateContact(contact).GetAwaiter().GetResult();
                            updated = true;
                            Clipboard.SetText(siteId);
                        }
                        else
                        {
                            notNull = true;
                        }
                        
                    }
                    catch(Exception ex)
                    {
                        updated = false;
                        sdkException = ex.Message;
                    }

                    if (updated)
                    {
                        MessageBox.Show(
                            $"Site ID has been generated for the selected contact and copied to clipboard.{Environment.NewLine}{Environment.NewLine}Site ID = {siteId}",
                            "Success", MessageBoxButton.OK,
                            MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        MessageBox.Show(
                            notNull
                                ? $"Site ID is not blank."
                                : $"An error occurred while updating Site ID:{Environment.NewLine}{sdkException}",
                            "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while updating Site ID for the selected Contact:{Environment.NewLine}{ex.Message}",
                    "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        
    }
}

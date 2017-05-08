using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace FinancialAdvisor.Helpers
{
    public static class StateHelper
    {
        public static async Task SetUserUiLanguageAsync(Activity activity, string UiCulture)
        {
            try
            {
                StateClient stateClient = activity.GetStateClient();
                BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
                userData.SetProperty<string>("UiCulture", UiCulture);               
                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
           

        public static string GetUserUiLanguage(Activity activity)
        {
            try
            {
                StateClient stateClient = activity.GetStateClient();
                BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
                var UiCulture = userData.GetProperty<string>("UiCulture");
                return UiCulture;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SetUserUiLanguage(IDialogContext context, string UiCulture)
        {
            try
            {
                context.UserData.SetValue("UiCulture", UiCulture);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetUserUiLanguage(IDialogContext context)
        {
            try
            {
                context.UserData.TryGetValue("UiCulture", out string result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
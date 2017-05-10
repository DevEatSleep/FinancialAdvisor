using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;

namespace FinancialAdvisor.Helpers
{
    public static class StateHelper
    {
        private static string _neutralLanguage;
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
                if(UiCulture == null)
                {
                    _neutralLanguage =
                        Assembly.GetExecutingAssembly().GetCustomAttribute<NeutralResourcesLanguageAttribute>().CultureName.Substring(0, 2);
                    UiCulture = _neutralLanguage;
                }
                
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

        public static string GetNeutralLanguage()
        {
            if(String.IsNullOrEmpty(_neutralLanguage))
                _neutralLanguage = Assembly.GetExecutingAssembly().GetCustomAttribute<NeutralResourcesLanguageAttribute>().CultureName.Substring(0, 2);
            return _neutralLanguage;
        }

        public static string GetUserUiLanguage(IDialogContext context)
        {
            try
            {
                context.UserData.TryGetValue("UiCulture", out string result);
                if (result == null)
                {
                    _neutralLanguage =
                        Assembly.GetExecutingAssembly().GetCustomAttribute<NeutralResourcesLanguageAttribute>().CultureName.Substring(0, 2);
                    result = _neutralLanguage;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
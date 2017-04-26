using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace FinancialAdvisor.Helpers
{
    public static class StateHelper
    {
       
        
        public static string GetUserLanguageCode(Activity activity)
        {
            try
            {
                StateClient stateClient = activity.GetStateClient();
                BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
               
                var languageCode = userData.GetProperty<string>("LanguageCode");

                return languageCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetUserLanguageCode(IDialogContext context)
        {
            try
            {
                string result;
                context.UserData.TryGetValue("LanguageCode", out result);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
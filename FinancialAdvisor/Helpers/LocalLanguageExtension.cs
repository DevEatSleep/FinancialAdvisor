using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FinancialAdvisor.Helpers
{
    public static class LocalLanguageExtension
    {
        public static async System.Threading.Tasks.Task<string> ToUserLocaleAsync(this string text, IDialogContext context)
        {
            var userLanguageCode = StateHelper.GetUserLanguageCode(context);
            if (userLanguageCode != "en")
            {
                text = await TranslationHandler.DoTranslation(text, "en", userLanguageCode);
            }

            return text;
        }
        public static string ToUserLocale(this string text, Activity activity)
        {
            var userLanguageCode = StateHelper.GetUserLanguageCode(activity);
            if (userLanguageCode != "en")
            {
                text = TranslationHandler.DoTranslation(text, "en", userLanguageCode).Result;
            }

            return text;
        }
    }
}
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public static async System.Threading.Tasks.Task<string> ToUserLocale(this string text, Activity activity)
        {
            var userLanguageCode = StateHelper.GetUserLanguageCode(activity);
            if (userLanguageCode != "en")
            {
                text = await TranslationHandler.DoTranslation(text, "en", userLanguageCode);
            }

            return text;
        }
    }
}
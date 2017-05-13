using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FinancialAdvisor.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FinancialAdvisor.Dialogs
{
    public static class Messages
    {
        private static async Task<string> WelcomeMessageCulturesAsync(IDialogContext context)
        {
            List<string> culturesName = new List<string>();
            CultureHelper.LoadCultures();

            var UiLanguage = StateHelper.GetUserUiLanguage(context);
            foreach (var item in CultureHelper.CulturesAvailable)
            {
                string languageName = item.EnglishName.Split(' ')[0]; 
                if(UiLanguage != "en")
                    languageName = await TranslationHelper.DoTranslation(languageName , "en", UiLanguage);
                culturesName.Add(languageName);               
            }
            return String.Join(", ", culturesName.ToArray());
        }

        private static async Task<string> WelcomeMessageCulturesAsync(Activity message)
        {
            List<string> culturesName = new List<string>();
            CultureHelper.LoadCultures();

            var UiLanguage = StateHelper.GetUserUiLanguage(message);           
            foreach (var item in CultureHelper.CulturesAvailable)
            {
                string languageName = item.EnglishName.Split(' ')[0];
                if (UiLanguage != "en")
                    languageName = await TranslationHelper.DoTranslation(languageName, "en", UiLanguage);
                culturesName.Add(languageName);
            }
            return String.Join(", ", culturesName.ToArray());
        }


        public async static Task WelcomeMessageAsync(IDialogContext context, string from)
        {
            var UiCulture = new CultureInfo(StateHelper.GetUserUiLanguage(context));
            if (UiCulture != null)
                Thread.CurrentThread.CurrentUICulture = UiCulture;
            var text = string.Concat(String.Format(Resources.Resource.WelcomeStringFirstLine, from),
                                Environment.NewLine,
                                Resources.Resource.WelcomeStringSecondLine,
                                Environment.NewLine,
                                Resources.Resource.WelcomeLanguageAvailable,
                                await WelcomeMessageCulturesAsync(context));
            await context.PostAsync(text);
        }

        public async static Task HelpMessageAsync(IDialogContext context)
        {
            var UiCulture = new CultureInfo(StateHelper.GetUserUiLanguage(context));
            if (UiCulture != null)
                Thread.CurrentThread.CurrentUICulture = UiCulture;

            var text = string.Concat(Resources.Resource.UsageFirstLine, Environment.NewLine,
                     Resources.Resource.UsageSecondLine, Environment.NewLine,
                     Resources.Resource.UsageThirdLine, Environment.NewLine,
                     Resources.Resource.UsageFourthLine, Environment.NewLine,
                     Resources.Resource.UsageFifthLine);
            await context.PostAsync(text);
        }

        public static async Task WelcomeMessageAsync(Activity message)
        {
            var UiCulture = new CultureInfo(StateHelper.GetUserUiLanguage(message));
            if (UiCulture != null)
                Thread.CurrentThread.CurrentUICulture = UiCulture;
            var text = string.Concat(string.Format(Resources.Resource.WelcomeStringFirstLine, message.From.Name),
                               Environment.NewLine,
                               Resources.Resource.WelcomeStringSecondLine,
                               Environment.NewLine,
                               Resources.Resource.WelcomeLanguageAvailable,
                               await WelcomeMessageCulturesAsync(message));
            
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            connector.Conversations.ReplyToActivity(message.CreateReply(text));
        }
    }
}
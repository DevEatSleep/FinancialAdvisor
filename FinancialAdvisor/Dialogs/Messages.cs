using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FinancialAdvisor.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FinancialAdvisor.Dialogs
{
    public static class Messages
    {
        public async static Task WelcomeMessageAsync(IDialogContext context, string from)
        {
            List<string> culturesName = new List<string>();
            CultureHelper.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            CultureHelper.LoadCultures();

            foreach (var item in CultureHelper.CulturesAvailable)
                culturesName.Add(item.DisplayName);

            var text = string.Concat(string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeStringFirstLine, from),
                                Environment.NewLine, string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeStringSecondLine),
                                Environment.NewLine,
                                string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeLanguageAvailable),
                                String.Join(", ", culturesName.ToArray()));


            await context.PostAsync(text);
        }

        public async static Task HelpMessageAsync(IDialogContext context)
        {
            var text = string.Concat(string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageFirstLine), "\n\n",
                     string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageSecondLine), "\n\n",
                     string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageThirdLine), "\n\n",
                     string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageFourthLine), "\n\n",
                        string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageFifthLine));
            await context.PostAsync(text);
        }

        public static void WelcomeMessage(Activity message, string from)
        {
            List<string> culturesName = new List<string>();
            CultureHelper.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            CultureHelper.LoadCultures();

            foreach (var item in CultureHelper.CulturesAvailable)
                culturesName.Add(item.DisplayName);

            var text = string.Concat(string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeStringFirstLine, from),
                               Environment.NewLine, string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeStringSecondLine),
                               Environment.NewLine,
                               string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeLanguageAvailable),
                               String.Join(", ", culturesName.ToArray()));

            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            connector.Conversations.ReplyToActivity(message.CreateReply(text));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BestMatchDialog;
using Microsoft.Bot.Builder.Dialogs;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    public class HelpDialog : BestMatchDialog<object>
    {
        [BestMatch(new string[] { "Help", "Usage" }, threshold: 0.5, ignoreCase: true, ignoreNonAlphaNumericCharacters: false)]
        public async Task WelcomeGreeting(IDialogContext context, string messageText)
        {
            await Messages.HelpMessageAsync(context);
            context.Done(true);
        }

        public override async Task NoMatchHandler(IDialogContext context, string messageText)
        {
            context.Done(false);
        }      
    }
}
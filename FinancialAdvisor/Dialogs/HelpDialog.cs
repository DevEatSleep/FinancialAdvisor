using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BestMatchDialog;
using FinancialAdvisor.Helpers;
using Microsoft.Bot.Builder.Dialogs;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    public class HelpDialog : BestMatchDialog<object>
    {
        [BestMatch(new string[] { "help", "usage", "?" }, threshold: 0.5, ignoreCase: true, ignoreNonAlphaNumericCharacters: false)]
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
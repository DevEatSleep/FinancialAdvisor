using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BestMatchDialog;
using FinancialAdvisor.Helpers;
using Microsoft.Bot.Builder.Dialogs;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    public class WelcomeDialog : BestMatchDialog<object>
    {        [BestMatch(new string[] { "hi", "hi There", "hello there", "hey", "hello",
            "hey there", "greetings", "Good morning", "good afternoon", "good evening", "good day" },
       threshold: 0.5, ignoreCase: true, ignoreNonAlphaNumericCharacters: false)]
        public async Task WelcomeGreeting(IDialogContext context, string messageText)
        {            
            await Messages.WelcomeMessageAsync(context, context.Activity.From.Name);
            context.Done(true);
        }

        [BestMatch(new string[] { "bye", "bye bye", "got to go",
            "see you later", "laters", "adios" },
       threshold: 0.5, ignoreCase: true, ignoreNonAlphaNumericCharacters: false)]
        public async Task FarewellGreeting(IDialogContext context, string messageText)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(StateHelper.GetUserUiLanguage(context));
            await context.PostAsync(Resources.Resource.ByeString);
            context.Done(true);
        }

        public override async Task NoMatchHandler(IDialogContext context, string messageText)
        {
            context.Done(false);
        }        
    }
}
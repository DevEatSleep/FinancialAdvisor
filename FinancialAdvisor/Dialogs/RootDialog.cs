using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FinancialAdvisor.Services;
using System.Web.Configuration;
using FinancialAdvisor.Entity;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {    
        private IWolframAlphaService _iwolframAlphaService = new WolframAlphaService();
               
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity.Text.ToLower() == "help")
            {
                await context.PostAsync("I can make conversions or calculations between currencies." + "\n\n" +
                  "like 'convert 100 USD in JPY' or 'add 100 USD and 10 JPY in EUR'" + "\n\n" +
                  "I can get stock quotes like 'get Microsoft price' " + "\n\n" +                  
                  "Be kind, I'm under development ;-)");
            }
            else
            {
                _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                var queryResult = _iwolframAlphaService.ExecQuery(activity.Text);
                await context.PostAsync(queryResult.ToString());
            }
            context.Wait(MessageReceivedAsync);
        }

        private Task ResumeAndPromptPlatformAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }
    }
}
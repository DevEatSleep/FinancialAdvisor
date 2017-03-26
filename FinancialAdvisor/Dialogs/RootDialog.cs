using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FinancialAdvisor.Services;
using System.Web.Configuration;

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
                       
            if(activity.Text.ToLower() != "help")
            { 
                if(!String.IsNullOrEmpty(activity.Text))
                {
                    _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                    var queryResult = _iwolframAlphaService.ExecQuery(activity.Text);
                    await context.PostAsync(queryResult.ToString());
                }               
            }
            else
            {
                await context.PostAsync("For the moment, I can only make conversions or calculations between currencies." + "\n\n" +
                    "like 'convert 100 USD in JPY' or 'add 100 USD and 10 JPY in EUR'" + "\n\n" +
                    "or '10 USD + 10 JPY - 10 CAD in EUR'" + "\n\n" +
                    "Many functions will come later, I'll keep you informed" + "\n\n" +
                    "Don't use often, I've a limitation in request to the API web server." + "\n\n" +
                    "Be kind, I'm under development ;-)");
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
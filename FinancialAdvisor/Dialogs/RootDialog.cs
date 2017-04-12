using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FinancialAdvisor.Services;
using System.Web.Configuration;
using FinancialAdvisor.Entity;
using FinancialAdvisor.Helpers;

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


            _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
            var queryResult = await _iwolframAlphaService.ExecQueryAsync(activity.Text);
            //TODO : parser les réponses et les traduire
            //var queryResultLocale = queryResult.ToUserLocaleAsync(context);
            await context.PostAsync(queryResult);

            context.Wait(MessageReceivedAsync);
        }

        private Task ResumeAndPromptPlatformAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }
    }
}
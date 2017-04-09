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

            if (activity.Text.ToLower() == Resources.Resource.HelpString)
            {
                await context.PostAsync(Resources.Resource.UsageFirstLine + "\n\n" +
                  Resources.Resource.UsageSecondLine + "\n\n" +
                  Resources.Resource.UsageThirdLine + "\n\n" +
                  Resources.Resource.UsageFourthLine);
            }
            else
            {
                _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                var queryResult = await _iwolframAlphaService.ExecQueryAsync(activity.Text);
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
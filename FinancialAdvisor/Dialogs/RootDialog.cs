using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FinancialAdvisor.Services;
using System.Web.Configuration;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    [LuisModel("e5b55528-76cd-48f7-8a61-9fd6fee485d8", "526e1166d5f34d178d96464bcb743ef8")]
    public class RootDialog : LuisDialog<object>
    {
        private IWolframAlphaService _iwolframAlphaService = new WolframAlphaService();
        private const string EntityMoneyName = "builtin.currency";

        //public Task StartAsync(IDialogContext context)
        //{            
        //    context.Wait(MessageReceivedAsync);
        //    return Task.CompletedTask;
        //}

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = Resources.Resource.UnknownQuery;
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("convert")]
        [LuisIntent("add")]
        [LuisIntent("substract")]
        [LuisIntent("divide")]
        public async Task GoToStation(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation moneyEntityRecommendation;

            if (result.TryFindEntity(EntityMoneyName, out moneyEntityRecommendation))
            {
                _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Money");
                //TODO : parser les réponses et les traduire
                //var queryResultLocale = queryResult.ToUserLocaleAsync(context);
                await context.PostAsync(queryResult);
            }
            context.Wait(this.MessageReceived);
        }
    }
}
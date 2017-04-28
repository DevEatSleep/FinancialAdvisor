using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FinancialAdvisor.Services;
using System.Web.Configuration;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using FinancialAdvisor.Helpers;
using System.Threading;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    [LuisModel("e5b55528-76cd-48f7-8a61-9fd6fee485d8", "526e1166d5f34d178d96464bcb743ef8")]
    public class RootDialog : LuisDialog<object>
    {
        private IWolframAlphaService _iwolframAlphaService = new WolframAlphaService();
        private const string EntityMoneyName = "builtin.currency";
        private const string EntityCompanyName = "company";
        private string[] EntitiesQuoteNames = { "Price", "Quote", "Value" };

        private string language;

        public RootDialog(string language)
        {
            this.language = language;
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            var cts = new CancellationTokenSource();
            await context.Forward(new WelcomeDialog(), GreetingDialogDoneAsync, await message, cts.Token);
        }

        private async Task GreetingDialogDoneAsync(IDialogContext context, IAwaitable<object> result)
        {
            var success = await result;
            if(!(bool)success)
                await context.PostAsync(Resources.Resource.UnknownQuery);

            context.Wait(MessageReceived);
        }

        [LuisIntent("help")]
        [LuisIntent("usage")]
        [LuisIntent("?")]
        public async Task Help(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            if (result.TopScoringIntent.Score < 0.5)
                await None(context, message, result);
            else
            {
                var cts = new CancellationTokenSource();
                await context.Forward(new HelpDialog(), HelpDialogDoneAsync, await message, cts.Token);
            }           
        }

        private async Task HelpDialogDoneAsync(IDialogContext context, IAwaitable<object> result)
        {
            var success = await result;
            if (!(bool)success)
                await context.PostAsync(Resources.Resource.UnknownQuery);

            context.Wait(MessageReceived);
        }

        [LuisIntent("get")]
        [LuisIntent("what")]
        public async Task GetStockQuote(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation QuoteEntityRecommendation;

            foreach (string entityQuoteName in EntitiesQuoteNames)
            {
                if (result.TryFindEntity(entityQuoteName, out QuoteEntityRecommendation))
                {
                    _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                    var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Data");

                    if (_iwolframAlphaService.HasValidData)
                    {
                        EntityRecommendation CompanyEntityRecommendation;

                        if (result.TryFindEntity(EntityCompanyName, out CompanyEntityRecommendation))
                        {
                            var formatQueryResult = _iwolframAlphaService.ParseQuote(queryResult, CompanyEntityRecommendation.Entity);

                            string translatedQueryResult;

                            if (language != "en")
                                translatedQueryResult = await TranslationHelper.DoTranslation(formatQueryResult, "en", language);
                            else
                                translatedQueryResult = formatQueryResult;

                            await context.PostAsync(translatedQueryResult);
                        }
                    }
                    else
                        await context.PostAsync(queryResult);
                    break;
                }
            }
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("convert")]
        [LuisIntent("+")]
        [LuisIntent("-")]
        [LuisIntent("*")]
        [LuisIntent("/")]
        public async Task DoMoneyCalc(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            if (result.TryFindEntity(EntityMoneyName, out EntityRecommendation moneyEntityRecommendation))
            {
                _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Money");

                //var queryResult = "euro94.22 (euros)"; ¥10860  (Japanese yen)

                if (_iwolframAlphaService.HasValidData)
                {
                    var formatQueryResult = _iwolframAlphaService.ParseMoney(queryResult);
                    string translatedQueryResult;

                    if (language != "en")
                        translatedQueryResult = await TranslationHelper.DoTranslation(formatQueryResult, "en", language);
                    else
                        translatedQueryResult = formatQueryResult;

                    await context.PostAsync(translatedQueryResult);
                }
                else
                    await context.PostAsync(queryResult);
            }
            context.Wait(this.MessageReceived);
        }       
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FinancialAdvisor.Services;
using System.Web.Configuration;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text.RegularExpressions;
using FinancialAdvisor.Helpers;

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


        [LuisIntent("get")]
        [LuisIntent("what")]
        public async Task GetStockQuote(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation QuoteEntityRecommendation;

            foreach(string entityQuoteName in EntitiesQuoteNames)
            {
                if (result.TryFindEntity(entityQuoteName, out QuoteEntityRecommendation))
                {
                    _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                    var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Data");

                    if (_iwolframAlphaService.HasValidData)
                    {
                        EntityRecommendation CompanyEntityRecommendation;
                        //TODO gérer company sur LUIS
                        if (result.TryFindEntity(EntityCompanyName, out CompanyEntityRecommendation))
                        {
                            var formatQueryResult = ParseQuote(queryResult, CompanyEntityRecommendation.Entity);
                            
                            string translatedQueryResult;

                            if (language != "en")
                                translatedQueryResult = await TranslationHandler.DoTranslation(formatQueryResult, "en", language);
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

            EntityRecommendation moneyEntityRecommendation;

            if (result.TryFindEntity(EntityMoneyName, out moneyEntityRecommendation))
            {
                _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Money");
               
                //var queryResult = "euro94.22 (euros)"; ¥10860  (Japanese yen)

                if(_iwolframAlphaService.HasValidData)
                {
                    var formatQueryResult = ParseMoney(queryResult);
                    string translatedQueryResult;

                    if (language != "en")
                        translatedQueryResult = await TranslationHandler.DoTranslation(formatQueryResult, "en", language);
                    else
                        translatedQueryResult = formatQueryResult;

                    await context.PostAsync(translatedQueryResult);
                }
                else
                    await context.PostAsync(queryResult);
            }
            context.Wait(this.MessageReceived);
        }
        //$64.95(MSFT | NASDAQ | 10:00:00 pm CEST | Thursday, April 13, 2017)
        private string ParseQuote(string queryResult, string companyName)
        {
            var quoteSentence = queryResult.Split("|".ToCharArray());

            return string.Concat(companyName, " price is ", quoteSentence[0]);

        }

        private string ParseMoney(string input)
        {
            Regex r = new Regex(@"([+-]?[0-9]*[.]?[0-9]+)([ ]*)(\(([^()]*)\))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(input);
            if (m.Success)
            {
                String value = m.Groups[1].ToString();
                String currency = m.Groups[4].ToString();

                return string.Concat(value, " ", currency);
            }
            else
                return input;            
        }
    }
}
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
using System.Globalization;
using System.Resources;

namespace FinancialAdvisor.Dialogs
{
    [Serializable]
    [LuisModel("e5b55528-76cd-48f7-8a61-9fd6fee485d8", "526e1166d5f34d178d96464bcb743ef8")]
    public class RootDialog : LuisDialog<object>
    {
        private IWolframAlphaService _iwolframAlphaService = new WolframAlphaService();

        private const string EntityMoneyName = "builtin.currency";
        private const string EntityCompanyName = "company";
        private const string LanguageEntityName = "language";
        private const string QueryLanguage = "en";
        private string[] EntitiesQuoteNames = { "price", "quotation", "value" };

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            await context.PostAsync(Resources.Resource.UnknownQuery);
            context.Wait(MessageReceived);
        }

        private async Task GreetingDialogDoneAsync(IDialogContext context, IAwaitable<object> result)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(StateHelper.GetUserUiLanguage(context));
            var success = await result;
            if (!(bool)success)
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
                await context.Forward(new HelpDialog(), DialogDoneAsync, await message, cts.Token);
            }
        }

        [LuisIntent("hi")]       
        public async Task Hi(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            if (result.TopScoringIntent.Score < 0.5)
                await None(context, message, result);
            else
            {
                var cts = new CancellationTokenSource();
                await context.Forward(new WelcomeDialog(), DialogDoneAsync, await message, cts.Token);
            }
        }

        private async Task DialogDoneAsync(IDialogContext context, IAwaitable<object> result)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(StateHelper.GetUserUiLanguage(context));
            var success = await result;
            if (!(bool)success)
                await context.PostAsync(Resources.Resource.UnknownQuery);

            context.Wait(MessageReceived);
        }

        [LuisIntent("speak")]
        public async Task Speak(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            if (result.TryFindEntity(LanguageEntityName, out EntityRecommendation languageEntityRecommendation))
            {
                var culture = CultureHelper.GetCulture(languageEntityRecommendation.Entity);
                if (culture == null)
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(StateHelper.GetUserUiLanguage(context));
                    string languageName = await TranslationHelper.DoTranslation(languageEntityRecommendation.Entity, QueryLanguage, StateHelper.GetUserUiLanguage(context));
                    await context.PostAsync(string.Format(Resources.Resource.LanguageUnaivalable, languageName));
                }
                else
                {
                    var currentLanguageName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                    StateHelper.SetUserUiLanguage(context, culture.TwoLetterISOLanguageName);
                    Thread.CurrentThread.CurrentUICulture = culture;
                    await context.PostAsync(string.Concat(Resources.Resource.NewLanguageString, " ", culture.NativeName));
                }                   
            }
            context.Wait(this.MessageReceived);
        }     

        [LuisIntent("what")]
        public async Task GetStockQuote(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            var currentLanguage = StateHelper.GetUserUiLanguage(context);
            
            foreach (string entityQuoteName in EntitiesQuoteNames)
            {
                if (result.TryFindEntity(entityQuoteName, out EntityRecommendation QuoteEntityRecommendation))
                {
                    if (currentLanguage != QueryLanguage)
                        message.Text = (await TranslationHelper.DoTranslation(message.Text, currentLanguage, QueryLanguage)).ToLower();

                    _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                    var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Data");

                    if (_iwolframAlphaService.HasValidData)
                    {
                        if (result.TryFindEntity(EntityCompanyName, out EntityRecommendation CompanyEntityRecommendation))
                        {
                            var formatQueryResult = _iwolframAlphaService.ParseQuote(queryResult, CompanyEntityRecommendation.Entity);

                            string translatedQueryResult;

                            if (currentLanguage != QueryLanguage)
                                translatedQueryResult = await TranslationHelper.DoTranslation(formatQueryResult, QueryLanguage, currentLanguage);
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
            var currentLanguage = StateHelper.GetUserUiLanguage(context);

            if (result.TryFindEntity(EntityMoneyName, out EntityRecommendation moneyEntityRecommendation))
            {
                if (currentLanguage != QueryLanguage)
                    message.Text = (await TranslationHelper.DoTranslation(message.Text, currentLanguage, QueryLanguage)).ToLower();

                _iwolframAlphaService.AppId = WebConfigurationManager.AppSettings["WolframAlphaAppId"];
                var queryResult = await _iwolframAlphaService.ExecQueryAsync(message.Text, "Money");

                //var queryResult = "euro94.22 (euros)"; ¥10860  (Japanese yen)

                if (_iwolframAlphaService.HasValidData)
                {
                    var formatQueryResult = _iwolframAlphaService.ParseMoney(queryResult);
                    string translatedQueryResult;

                    if (currentLanguage != QueryLanguage)
                        translatedQueryResult = await TranslationHelper.DoTranslation(formatQueryResult, QueryLanguage, currentLanguage);
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
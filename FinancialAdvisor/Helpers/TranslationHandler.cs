using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using System.Web.Configuration;
using TranslatorService;

namespace FinancialAdvisor.Helpers
{
    public static class TranslationHandler
    {
        public static async Task DetectAndSetUserLanguageCode(Activity activity)
        {
            var inputLanguageCode = DoLanguageDetection(activity.Text);
            await StateHelper.SetUserLanguageCode(activity, inputLanguageCode.Result);
        }

        public static async Task<string> DetectAndTranslateAsync(Activity activity)
        {
            //detect language
            //update state for current user to detected language

            var inputLanguageCode = await DoLanguageDetection(activity.Text);
            //await StateHelper.SetUserLanguageCode(activity, inputLanguageCode.Result);

            if (inputLanguageCode != "en")
            {

                return await DoTranslation(activity.Text, inputLanguageCode, "en");

            }
            return await new Task<string>(() => activity.Text);
        }

        public static Task<string> DoTranslation(string inputText, string inputLocale, string outputLocale)
        {
            var translator = new TranslatorServiceClient(WebConfigurationManager.AppSettings["TextTranslatorId"]);
            return translator.TranslateAsync(inputText, inputLocale, outputLocale);
        }

        public static Task<string> DoLanguageDetection(string input)
        {
            var translator = new TranslatorServiceClient(WebConfigurationManager.AppSettings["TextTranslatorId"]);
            return translator.DetectLanguageAsync(input);
        }
    }
}
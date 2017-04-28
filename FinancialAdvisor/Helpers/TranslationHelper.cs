using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.Bot.Connector;
using TranslatorService;

namespace FinancialAdvisor.Helpers
{
    public static class TranslationHelper
    {
        public static Task<string> DoTranslation(string inputText, string inputLocale, string outputLocale)
        {
            var translator = new TranslatorServiceClient(WebConfigurationManager.AppSettings["TextTranslatorId"]);
            return translator.TranslateAsync(inputText, inputLocale, outputLocale);
        }

        public static Task<string> DoLanguageDetectionAsync(string input)
        {
            var translator = new TranslatorServiceClient(WebConfigurationManager.AppSettings["TextTranslatorId"]);
            return translator.DetectLanguageAsync(input);
        }

        public static async Task<string> DetectAndTranslateAsync(Activity activity)
        {
            //detect language
            //update state for current user to detected language
            var inputLanguageCode = await DoLanguageDetectionAsync(activity.Text);

            await StateHelper.SetUserLanguageCode(activity, inputLanguageCode);

            if (inputLanguageCode.ToLower() != "en")
            {
                return await DoTranslation(activity.Text, inputLanguageCode, "en");
            }
            return activity.Text;
        }
    }
}
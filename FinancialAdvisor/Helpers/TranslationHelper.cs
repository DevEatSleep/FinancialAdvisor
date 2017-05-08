using System.Globalization;
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

        public static async Task<string> GetNativeLanguageNameAsync(string languageName, CultureInfo currentCulture)
        {
            return await TranslationHelper.DoTranslation(languageName, currentCulture.TwoLetterISOLanguageName, "en");
        }
    }
}
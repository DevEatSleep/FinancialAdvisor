using System.Threading.Tasks;
using System.Web.Configuration;
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

        public static Task<string> DoLanguageDetection(string input)
        {
            var translator = new TranslatorServiceClient(WebConfigurationManager.AppSettings["TextTranslatorId"]);
            return translator.DetectLanguageAsync(input);
        }
    }
}
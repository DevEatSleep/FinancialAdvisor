using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;

namespace FinancialAdvisor.Helpers
{
    public static class CultureHelper
    {
        static List<CultureInfo> _culturesAvailable = new List<CultureInfo>();
        public static List<CultureInfo> CulturesAvailable { get => _culturesAvailable; set => _culturesAvailable = value; }
        static CultureInfo _currentCulture;
        public static CultureInfo CurrentCulture { get => _currentCulture; set => _currentCulture = value; }

        public static async Task<bool> ChangeCultureAsync(string languageName)
        {
            ResourceManager rm = new ResourceManager(typeof(Resources.Resource));

            var languageNameInEnglish = await TranslationHelper.DoTranslation(languageName, _currentCulture.TwoLetterISOLanguageName, "en");

            foreach (CultureInfo culture in _culturesAvailable)
            {
                try
                {
                    if (culture.EnglishName.ToLower().Contains( languageNameInEnglish.ToLower()))
                    {
                        Resources.Resource.Culture = culture;
                        _currentCulture = culture;
                        return true;
                    }
                }
                catch (CultureNotFoundException)
                {
                }
            }
            return false;
        }

        public static void LoadCultures()
        {
            if (_culturesAvailable.Count > 0)
                _culturesAvailable.Clear();

            ResourceManager rm = new ResourceManager(typeof(Resources.Resource));
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (CultureInfo culture in cultures)
            {
                try
                {
                    ResourceSet rs = rm.GetResourceSet(culture, true, false);
                    if (rs != null && culture.ThreeLetterWindowsLanguageName != "IVL")
                        _culturesAvailable.Add(culture);
                }
                catch (CultureNotFoundException)
                {
                }
            }
        }
    }
}
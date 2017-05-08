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
        
        public static CultureInfo GetCulture(string EnglishName)
        {
            foreach (CultureInfo info in _culturesAvailable)
            {
                if (info.EnglishName.ToLower().Contains(EnglishName.ToLower()))
                    return info;
            }
            return null;
        }

        public static void LoadCultures()
        {
            if (_culturesAvailable.Count > 0)
                _culturesAvailable.Clear();

            ResourceManager rm = new ResourceManager(typeof(Resources.Resource));
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
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
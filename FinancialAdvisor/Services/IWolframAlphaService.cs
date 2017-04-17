using System.Threading.Tasks;

namespace FinancialAdvisor.Services
{
    public interface IWolframAlphaService
    {
        string AppId { get; set; }
        bool HasValidData { get; set; }
        Task<string> ExecQueryAsync(string query, string scanner);
    }
}
namespace FinancialAdvisor.Services
{
    public interface IWolframAlphaService
    {
        string AppId { get; set; }
        string ExecQuery(string query);
    }
}
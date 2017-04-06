using FinancialAdvisor.Entity;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WolframAlphaNET;
using WolframAlphaNET.Misc;
using WolframAlphaNET.Objects;

namespace FinancialAdvisor.Services
{
    [Serializable]
    public class WolframAlphaService : IWolframAlphaService
    {
        private string _appId = string.Empty;
        public string AppId { get => _appId; set => _appId = value; }

        public string ExecQuery(string query)
        {
            IRequestLimiter _requestLimiter = ServiceResolver.Get<IRequestLimiter>();
                RequestLimitEntity entity = _requestLimiter.Read();

            if (entity.LastQueryDate.Month == DateTime.Now.Month && entity.QueriesNumber == 2000)
                return Resources.Resource.NoMoreQueriesString;

            if (DateTime.Now.Month > entity.LastQueryDate.Month)
                _requestLimiter.Update(entity, DateTime.Now, 1);

            if (string.IsNullOrEmpty(query))
                return Resources.Resource.EmptyQueryString;

            WolframAlpha wolfram = new WolframAlpha(_appId);
            wolfram.ScanTimeout = 1; //We set ScanTimeout really low to get a quick answer. See RecalculateResults() below.
            wolfram.UseTLS = true; //Use encryption
            wolfram.Scanners.Add("Money");
            wolfram.Scanners.Add("Data");

            _requestLimiter.Update(entity, DateTime.Now, entity.QueriesNumber + 1);

            QueryResult results = wolfram.Query(query);

            if (results.ParseTimedout)
                results.RecalculateResults();

            if (results.Error != null)
                return Resources.Resource.ErrorString + results.Error.Message;

            if (results.Warnings != null)
            {
                if (results.Warnings.SpellCheck != null)
                {
                    ExecQuery(results.Warnings.SpellCheck.Text);
                }
            }

            if (results.DidYouMean.HasElements())
            {
                List<string> meanList = new List<string>
                {
                    Resources.Resource.MeanString
                };
                foreach (DidYouMean didYouMean in results.DidYouMean)
                {
                    meanList.Add(didYouMean.Value);
                }
                return meanList.Aggregate((i, j) => i + "," + j).ToString();
            }
            else
            {
                Pod primaryPod = results.GetPrimaryPod();
                if (primaryPod != null)
                {
                    if (primaryPod.SubPods.HasElements())
                    {
                        List<string> resultList = new List<string>();
                        foreach (SubPod subPod in primaryPod.SubPods)
                        {
                            resultList.Add(subPod.Plaintext);
                        }
                        return resultList.Aggregate((i, j) => i + "," + j).ToString();
                    }
                }
                else
                    return Resources.Resource.UnknownQuery;
            }
            return Resources.Resource.UnknownQuery;
        }
    }
}
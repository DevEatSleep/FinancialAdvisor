using FinancialAdvisor.Entity;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using TranslatorService;
using WolframAlphaNET;
using WolframAlphaNET.Misc;
using WolframAlphaNET.Objects;
using System.Threading.Tasks;
using Autofac;

namespace FinancialAdvisor.Services
{
    [Serializable]
    public class WolframAlphaService : IWolframAlphaService
    {
        private string _appId = string.Empty;
        public string AppId { get => _appId; set => _appId = value; }
        

        public async Task<string> ExecQueryAsync(string query)
        {
            IRequestLimiter _requestLimiter = ServiceResolver.Get<IRequestLimiter>();
            
            RequestLimitEntity wolframEntity = _requestLimiter.Read("Wolfram", "FinancialAdvisor");
            RequestLimitEntity translatorEntity = _requestLimiter.Read("CognitiveServices", "TextTranslator");

            if (wolframEntity.LastQueryDate.Month == DateTime.Now.Month && wolframEntity.QueriesNumber == 2000)
                return Resources.Resource.NoMoreQueriesString;

            if (DateTime.Now.Month > wolframEntity.LastQueryDate.Month)
                _requestLimiter.Update(wolframEntity, DateTime.Now, 1);

            if (translatorEntity.LastQueryDate.Month == DateTime.Now.Month && translatorEntity.QueriesNumber == 2000000)
                return Resources.Resource.NoMoreQueriesString;

            if (DateTime.Now.Month > translatorEntity.LastQueryDate.Month)
                _requestLimiter.Update(translatorEntity, DateTime.Now, query.Length);

            if (string.IsNullOrEmpty(query))
                return Resources.Resource.EmptyQueryString;

            WolframAlpha wolfram = new WolframAlpha(_appId);
            wolfram.ScanTimeout = 1; //We set ScanTimeout really low to get a quick answer. See RecalculateResults() below.
            wolfram.UseTLS = true; //Use encryption
            wolfram.Scanners.Add("Money");
            wolfram.Scanners.Add("Data");

            _requestLimiter.Update(wolframEntity, DateTime.Now, wolframEntity.QueriesNumber + 1);
            _requestLimiter.Update(translatorEntity, DateTime.Now, translatorEntity.QueriesNumber + query.Length);

            QueryResult results = wolfram.Query(query);

            if (results.ParseTimedout)
                results.RecalculateResults();

            if (results.Error != null)
                return Resources.Resource.ErrorString + results.Error.Message;

            if (results.Warnings != null)
            {
                if (results.Warnings.SpellCheck != null)
                {
                    await ExecQueryAsync(results.Warnings.SpellCheck.Text);
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
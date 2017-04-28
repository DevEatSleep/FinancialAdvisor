using FinancialAdvisor.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using WolframAlphaNET;
using WolframAlphaNET.Misc;
using WolframAlphaNET.Objects;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FinancialAdvisor.Services
{
    [Serializable]
    public class WolframAlphaService : IWolframAlphaService
    {
        private string _appId = string.Empty;
        public string AppId { get => _appId; set => _appId = value; }
        private bool _hasValidData;
        public bool HasValidData { get => _hasValidData; set => _hasValidData = value; }
        

        public async Task<string> ExecQueryAsync(string query, string scanner)
        {
            _hasValidData = false;

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

            WolframAlpha wolfram = new WolframAlpha(_appId)
            {
                //ScanTimeout = 1, //We set ScanTimeout really low to get a quick answer. See RecalculateResults() below.
                UseTLS = true //Use encryption
            };
            wolfram.Scanners.Add(scanner);
            //wolfram.Scanners.Add("Data");

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
                    await ExecQueryAsync(results.Warnings.SpellCheck.Text, scanner);
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
                        _hasValidData = true;
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
        //$64.95(MSFT | NASDAQ | 10:00:00 pm CEST | Thursday, April 13, 2017)
        public string ParseQuote(string queryResult, string companyName)
        {
            var quoteSentence = queryResult.Split("|".ToCharArray());

            Regex r = new Regex(@"([+-]?[0-9]*[.]?[0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(quoteSentence[0]);
            if (m.Success)
            {
                return string.Concat("The price of ", companyName, " is ", m.Groups[0]);
            }
            else
                return queryResult;
        }

        public string ParseMoney(string input)
        {
            Regex r = new Regex(@"([+-]?[0-9]*[.]?[0-9]+)([ ]*)(\(([^()]*)\))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(input);
            if (m.Success)
            {
                String value = m.Groups[1].ToString();
                String currency = m.Groups[4].ToString();

                return string.Concat(value, " ", currency);
            }
            else
                return input;
        }
    }
}
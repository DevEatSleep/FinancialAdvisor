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
        //private TelemetryClient telemetry = new TelemetryClient();
        private string _appId = string.Empty;        
        public string AppId { get => _appId; set => _appId = value; }
       
        public string ExecQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
                return "Empty query ?";

            WolframAlpha wolfram = new WolframAlpha(_appId);
            //wolfram.ScanTimeout = 0.1f; //We set ScanTimeout really low to get a quick answer. See RecalculateResults() below.
            wolfram.UseTLS = true; //Use encryption
                                   // wolfram.Scanners.Add("identity");                     

            QueryResult results = wolfram.Query(query);

            //if (results.Success)
            //{
            //    telemetry.TrackPageView();
            //}

            var source = string.Empty;
            if (results.Sources != null)
                if (results.Sources.Count > 0)
                    source = results.Sources[0].Text;

            if (source != "Financial data")
                return "Your question is off topic :-(";

            if (results.Error != null)
                return "Woops, where was an error: " + results.Error.Message;

            if (results.Warnings != null)
            {
                if (results.Warnings.Translation != null)
                {
                    ExecQuery(results.Warnings.Translation.Text);
                }
                    //return "Translation warning: " + results.Warnings.Translation.Text;

                if (results.Warnings.SpellCheck != null)
                {
                    ExecQuery(results.Warnings.SpellCheck.Text);
                }
                    //return "Spellcheck warning: " + results.Warnings.SpellCheck.Text;
            }

            if (results.DidYouMean.HasElements())
            {
                List<string> meanList = new List<string>
                {
                    "Did you mean: "
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
                    return "Sorry, I didn't understand, please type 'Help' to see examples";
            }
            return "Sorry, I didn't understand, please type 'Help' to see examples";
        }
    }
}
//Results are split into "pods" that contain information. Those pods can also have subpods.




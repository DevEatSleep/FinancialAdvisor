using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace FinancialAdvisor.Entity
{
    public interface IRequestLimiter
    {
        CloudTableClient TableClient { get; set; }

        RequestLimitEntity Read(string partitionKey, string rowKey);
        void Update(RequestLimitEntity entity, DateTime RequestDatetime, int QueriesNumber);
    }
}
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FinancialAdvisor.Entity
{
    public class RequestLimiter : IRequestLimiter
    {       
        private CloudTableClient tableClient;
        private CloudTable table;

        public RequestLimiter()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            TableClient = storageAccount.CreateCloudTableClient();
        }

        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }

        public RequestLimitEntity Read()
        {
            table = TableClient.GetTableReference("RequestLimit");
            TableOperation tableOperation = TableOperation.Retrieve<RequestLimitEntity>("Wolfram", "FinancialAdvisor");
            TableResult retrievedResult = table.Execute(tableOperation);
            return (RequestLimitEntity)retrievedResult.Result;
        }

        public void Update(RequestLimitEntity entity, DateTime RequestDatetime, Int32 QueriesNumber)
        {
            if (entity != null)
            {
                entity.LastQueryDate = RequestDatetime;
                entity.QueriesNumber = QueriesNumber;
                TableOperation updateOperation = TableOperation.Replace(entity);               
                table.Execute(updateOperation);
            }
        }
    }
}
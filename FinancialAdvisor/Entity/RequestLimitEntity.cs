using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialAdvisor.Entity
{
    public class RequestLimitEntity : TableEntity
    {
        public RequestLimitEntity(string Provider, string AppName)
        {
            this.PartitionKey = Provider;
            this.RowKey = AppName;
        }

        public RequestLimitEntity() { }

        public DateTime LastQueryDate{ get; set; }

        public Int32 QueriesNumber { get; set; }
    }
}
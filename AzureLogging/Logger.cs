using AzureLogging.Data;
using IISTask;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using smarx.WazStorageExtensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureLogging
{
    public static class Logger
    {
        public static LogResult Log(Exception exception)
        {
            return Log(exception, null);
        }

        public static LogResult Log(Exception exception, HttpRequestMessage request)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureLogging.LoggingAccount"]);

            bool useIncrementalIds = bool.Parse(ConfigurationManager.AppSettings["AzureLogging.UseIncrementalIds"]);

            string id = Guid.NewGuid().ToString();
            if (useIncrementalIds)
            {
                var blob = account.CreateCloudBlobClient().GetContainerReference("leases").GetBlobReference("logging-" + Environment.MachineName + ".lease");

                int? intId = null;
                while (!intId.HasValue)
                {
                    using (AutoRenewLease arl = new AutoRenewLease(blob))
                    {
                        if (arl.HasLease)
                        {
                            blob.FetchAttributes();
                            if (blob.Metadata["lastId"] == null)
                            {
                                blob.Metadata["lastId"] = "0";
                            }
                            intId = int.Parse(blob.Metadata["lastId"]) + 1;
                            blob.Metadata["lastId"] = intId.Value.ToString();
                            blob.SetMetadata(arl.LeaseId);
                        }
                        else
                        {
                            Thread.Sleep(200);
                        }
                    }
                }
                id = intId.Value.ToString();
            }

            LogEntry entry = new LogEntry(Environment.MachineName, id, exception, request);
            return new LogResult(id, IISTaskFactory.StartNew(() => DoLog(entry, account, id, exception, request)));
        }

        private static void DoLog(LogEntry entry, CloudStorageAccount account, string id, Exception exception, HttpRequestMessage request)
        {
            CloudTableClient tableClient = account.CreateCloudTableClient();
            tableClient.CreateTableIfNotExist("Logs");
            TableServiceContext tableContext = tableClient.GetDataServiceContext();
            tableContext.AddObject("Logs", entry);
            tableContext.SaveChangesWithRetries();
        }
    }
}

using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AzureLogging.Data
{
    class LogEntry : TableServiceEntity
    {
        public DateTime LoggedAt { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionData { get; set; }
        public string RequestIP { get; set; }
        public string RequestHeaders { get; set; }
        public string RequestUri { get; set; }
        public string RequestBody { get; set; }

        public LogEntry()
        {
        }

        public LogEntry(string partitionKey, string rowKey, Exception exception, HttpRequestMessage request)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            LoggedAt = DateTime.UtcNow;

            ExceptionType = exception.GetType().ToString();
            ExceptionData = "";
            Exception currentExc = exception;
            do
            {
                ExceptionData += "Exception Type: " + currentExc.GetType().ToString() + "\n";
                ExceptionData += "Exception Message: " + currentExc.Message + "\n";
                ExceptionData += "Exception Stack Trace: " + currentExc.StackTrace + "\n\n";
                ExceptionData += "================ INNER EXCEPTION ================\n\n";
            } while ((currentExc = currentExc.InnerException) != null);

            if (request != null)
            {
                RequestIP = GetClientIp(request);
                RequestUri = request.RequestUri.ToString();
                RequestBody = request.Content.ReadAsStringAsync().Result.Replace('&', '\n');

                RequestHeaders = "";
                foreach (var header in request.Headers)
                {
                    RequestHeaders += header.Key + ": " + header.Value.Aggregate((x, y) => x + ", " + y) + "\n";
                }
            }
        }

        private string GetClientIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop;
                prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureLogging
{
    public class LogResult
    {
        public string Id { get; set; }
        public Task LoggingTask { get; set; }

        public LogResult(string id, Task task)
        {
            Id = id;
            LoggingTask = task;
        }
    }
}

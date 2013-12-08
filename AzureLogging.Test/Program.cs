using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureLogging.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int p = 1 - 1;
                int j = 2 / p;
            }
            catch (Exception exc)
            {
                LogResult result = Logger.Log(exc);
                //result.LoggingTask.Wait();
            }
        }
    }
}

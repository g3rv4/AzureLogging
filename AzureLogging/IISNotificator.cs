using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AzureLogging
{
    public class IISNotificator : IRegisteredObject
    {
        public void Stop(bool immediate)
        {
            //do nothing
        }

        public void Started()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Ended()
        {
            HostingEnvironment.UnregisterObject(this);
        }
    }
}

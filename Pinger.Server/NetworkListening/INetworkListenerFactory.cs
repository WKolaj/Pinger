using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server.NetworkListening
{
    public interface INetworkListenerFactory
    {
		INetworkListener CreateNetworkListener(string protocol, int port, string? ipAddress = null);
    }
}

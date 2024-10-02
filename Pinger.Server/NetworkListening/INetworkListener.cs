using Pinger.Server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server.NetworkListening
{
    public interface INetworkListener
	{
		public IPAddress Address { get; }
		public int Port { get; }

        Task StartListening(Func<ClientInfo, string, NetworkStream, Task> onIncomingData, CancellationToken token);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Client.NetworkListening
{
	public interface INetworkClientFactory
	{
		INetworkClient CreateNetworkClient(
			string protocol, string serverIpAddress, int serverPort);
	}
}

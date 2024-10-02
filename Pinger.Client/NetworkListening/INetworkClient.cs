using Pinger.Server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Client.NetworkListening
{
	public interface INetworkClient: IDisposable
	{
		string Address { get; }

		int Port { get; }

		ClientInfo ServerInfo { get; }

		Task<string?> WaitForMessageFromServer();

		Task SendMessage(string message);

		Task Start();

		Task Stop();
	}
}

using Microsoft.Extensions.Logging;
using Pinger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Client.NetworkListening
{
	internal class NetworkClientFactory : INetworkClientFactory
	{
		private readonly ILogger<TCPNetworkClient> _tcpListnerlogger;

		public NetworkClientFactory(
			ILogger<TCPNetworkClient> tcpListenerlogger)
		{
			this._tcpListnerlogger = tcpListenerlogger;
		}

		public INetworkClient CreateNetworkClient(
			string protocol, string serverIpAddress, int serverPort)
		{
			Protocols.Validate(protocol);

			if (protocol == Protocols.TCP)
			{
				return new TCPNetworkClient(
					this._tcpListnerlogger,
					serverIpAddress,
					serverPort);
			}

			throw new ArgumentException($"Client for protocol '{nameof(protocol)}' not found!");
		}
	}
}

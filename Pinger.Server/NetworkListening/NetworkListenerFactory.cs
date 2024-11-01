﻿using Microsoft.Extensions.Logging;
using Pinger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server.NetworkListening
{
	public class NetworkListenerFactory : INetworkListenerFactory
	{
		private readonly ILogger<TCPNetworkListener> _tcpListnerlogger;

		public NetworkListenerFactory(
			ILogger<TCPNetworkListener> tcpListnerlogger)
		{
			this._tcpListnerlogger = tcpListnerlogger;
		}

		public INetworkListener CreateNetworkListener(
			string protocol, int port, string? ipAddress = null)
		{
			Protocols.Validate(protocol);

			var serverAddress = GetIpAddressForServer(ipAddress);

			if (protocol == Protocols.TCP)
			{
				return new TCPNetworkListener(this._tcpListnerlogger, port, serverAddress);
			}

			throw new ArgumentException($"Listner for protocol '{nameof(protocol)}' not found!");
		}

		private IPAddress GetIpAddressForServer(string? ipAddress)
		{
			if(String.IsNullOrWhiteSpace(ipAddress))
				return IPAddress.Any;

			return IPAddress.Parse(ipAddress);
		}
	}
}

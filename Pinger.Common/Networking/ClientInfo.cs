using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server.Networking
{
	public class ClientInfo
	{
		public string Address { get; }
		public int Port { get; }

		public ClientInfo(
			string ipAddress,
			int port) 
		{
			Address = ipAddress;
			Port = port;
		}

        public ClientInfo(EndPoint endPoint)
        {
			var ipEndpoint = endPoint as IPEndPoint;

			if (ipEndpoint == null)
				throw new ArgumentException("Endpoint must be an IP endpoint!");

			this.Address = ipEndpoint.Address.ToString();
			this.Port = ipEndpoint.Port;
		}

		public override string ToString()
		{
			return $"{Address}:{Port}";
		}

	}
}

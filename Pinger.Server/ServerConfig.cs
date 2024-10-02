using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server
{
	public class ServerConfig
	{
		public const string SectionName = "Server";

		public int Port { get; set; } = 0;

		public string Protocol {  get; set; } = String.Empty;

		public string? IPAddress { get; set; } = null;
	}
}

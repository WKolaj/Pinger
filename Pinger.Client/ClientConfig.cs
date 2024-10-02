using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Client
{
	public class ClientConfig
	{
		public const string SectionName = "Client";

		public int ServerPort { get; set; } = 0;

		public string ServerAddress { get; set; } = String.Empty;

		public string Protocol { get; set; } = String.Empty;
	}
}

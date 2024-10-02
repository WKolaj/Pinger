using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Common
{
	public static class Protocols
	{
		public const string TCP = "TCP";
		public const string UDP = "UDP";

		public static void Validate(string value)
		{
			if (value != TCP && value != UDP)
			{
				throw new ArgumentException(
					$"Protocol '{value}' is invalid! It can be '{TCP}' or '{UDP}'!");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pinger.Common.Networking
{
	public static class NetworkStreamExtesions
	{
		public static async Task<string> ReadMessageFromStream(
			this NetworkStream stream, int messageBufferLength)
		{
			byte[] buffer = new byte[messageBufferLength];

			int bytesRead = await stream.ReadAsync(buffer, 0, messageBufferLength);

			var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

			return message;
		}

		public static async Task FlushMessageToStream(
			this NetworkStream stream, string message)
		{
			var bytes = Encoding.UTF8.GetBytes(message);

			await stream.WriteAsync(bytes, 0, bytes.Length);
			await stream.FlushAsync();
		}
	}
}

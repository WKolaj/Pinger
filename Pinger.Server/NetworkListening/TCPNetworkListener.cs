using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using Pinger.Server.Networking;

namespace Pinger.Server.NetworkListening
{
	public class TCPNetworkListener : INetworkListener
	{
		private readonly ILogger<TCPNetworkListener> _logger;

		public int Port { get; }
		public IPAddress Address { get; }

		public TCPNetworkListener(ILogger<TCPNetworkListener> logger, int port) :
			this(logger, port, IPAddress.Any)
		{

		}

		public TCPNetworkListener(ILogger<TCPNetworkListener> logger, int port, IPAddress ipAddress)
		{
			_logger = logger;
			Port = port;
			Address = ipAddress;
		}

		public async Task StartListening(Func<ClientInfo, string, NetworkStream, Task> onIncomingData, CancellationToken token)
		{
			var serverInfo = new ClientInfo(Address.ToString(), Port);

			using (var server = new TcpListener(Address, Port))
			{
				_logger.LogTrace($"Starting TCP listining on '{serverInfo}'");

				server.Start();

				_logger.LogTrace($"Listening TCP started on '{serverInfo}'");

				while (!token.IsCancellationRequested)
				{
					_logger.LogTrace($"Waiting for new clients to connect...");

					TcpClient client = await WaitForClientToConnect(server);

					_logger.LogTrace($"New client connected.");

					Task.Run(() => HandleClient(client, onIncomingData, token));
				}

				_logger.LogTrace($"Stopping TCP listening on '{serverInfo}'");

				server.Stop();

				_logger.LogTrace($"TCP listening stopped '{serverInfo}'");
			}

			_logger.LogTrace($"TCP server closed '{serverInfo}'");
		}

		private static async Task<TcpClient> WaitForClientToConnect(TcpListener server)
		{
			return await server.AcceptTcpClientAsync().ConfigureAwait(false);
		}

		private async Task HandleClient(TcpClient client, Func<ClientInfo, string, NetworkStream, Task> onIncomingData, CancellationToken token)
		{
			try
			{
				var clientInfo = new ClientInfo(client.Client.RemoteEndPoint!);

				_logger.LogTrace($"Connected with client '{clientInfo}'");

				using (var streamWithClient = client.GetStream())
				{
					_logger.LogTrace($"Waiting for data form client '{clientInfo}'");

					while (!token.IsCancellationRequested && client.Connected)
					{
						var data = await WaitForNewDataFromTheClient(client, streamWithClient, token);

						if (data != null)
						{
							_logger.LogTrace($"New Data from client '{clientInfo}'");

							await onIncomingData(clientInfo, data, streamWithClient);

							_logger.LogTrace($"Waiting for data form client '{clientInfo}'");
						}
					}

					_logger.LogTrace($"Closing connection with client '{clientInfo}'");
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError(ex, "An exception occured during reading for the client");
			}
			finally
			{
				var infoAboutClient = client.Client.RemoteEndPoint as IPEndPoint;

				client.Dispose();

				_logger.LogTrace($"Connection closed with client '{infoAboutClient?.Address}' on Port '{infoAboutClient?.Port}'");
			}
		}

		private static async Task<string?> WaitForNewDataFromTheClient(TcpClient client, NetworkStream clientStream, CancellationToken token)
		{
			string? data = null;

			while (data == null && !token.IsCancellationRequested)
			{
				byte[] buffer = new byte[client.ReceiveBufferSize];

				int bytesRead = await clientStream.ReadAsync(buffer, 0, client.ReceiveBufferSize);

				data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

				await Task.Delay(100);
			}

			return data;
		}
	}
}

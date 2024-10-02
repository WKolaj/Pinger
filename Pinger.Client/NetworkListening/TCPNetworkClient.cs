using Microsoft.Extensions.Logging;
using Pinger.Server.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Client.NetworkListening
{
	public class TCPNetworkClient : INetworkClient
	{
		private readonly ILogger<TCPNetworkClient> _logger;

		public string Address { get; }
		public int Port { get; }

		public ClientInfo ServerInfo { get; }

		private TcpClient? _tcpClient = null;
		private NetworkStream? _networkStream = null;

		public TCPNetworkClient(
			ILogger<TCPNetworkClient> logger,
			string serverIpAddress,
			int serverPort)
		{
			this._logger = logger;
			this.Address = serverIpAddress;
			this.Port = serverPort;
			this.ServerInfo = new ClientInfo(serverIpAddress, serverPort);
		}

		public Task Start()
		{
			EnsureClientNotStarted();

			try
			{
				this._logger.LogTrace($"Staring client.");

				this._logger.LogTrace($"Trying to connect to '{this.ServerInfo}'");

				var client = new TcpClient(Address, Port);

				this._logger.LogTrace($"Client connected to '{this.ServerInfo}'");

				this._tcpClient = client;
				this._networkStream = client.GetStream();

			}
			catch (Exception ex)
			{
				this._logger.LogError(ex, $"Error when connection to '{this.ServerInfo}'");

				throw;
			}

			return Task.CompletedTask;
		}

		public Task Stop()
		{
			EnsureClientStarted(this._tcpClient);

			this._logger.LogTrace($"Closing connection with '{this.ServerInfo}'");

			this._networkStream?.Close();
			this._tcpClient.Close();

			this._networkStream = null;
			this._tcpClient = null;

			this._logger.LogTrace($"Connection closed with '{this.ServerInfo}'");

			return Task.CompletedTask;
		}

		public async Task SendMessage(string message)
		{
			EnsureClientStarted(this._tcpClient);

			this._logger.LogTrace($"Sending message '{message}' to '{this.ServerInfo}'");

			var bytes = Encoding.UTF8.GetBytes(message);

			await this._networkStream!.WriteAsync(bytes, 0, bytes.Length);

			this._logger.LogTrace($"Message '{message}' send successfully to '{this.ServerInfo}'");
		}

		public async Task<string?> WaitForMessageFromServer()
		{
			EnsureClientStarted(this._tcpClient);

			this._logger.LogTrace($"Waiting for message from '{this.ServerInfo}'");

			byte[] buffer = new byte[this._tcpClient.ReceiveBufferSize];

			int bytesRead = await this._networkStream!.ReadAsync(buffer, 0, this._tcpClient.ReceiveBufferSize);

			var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

			this._logger.LogTrace($"Message from '{this.ServerInfo}' recieved");

			return message;
		}


		public void Dispose()
		{
			this._logger.LogTrace($"Clearning resources of connection to '{this.ServerInfo}'");

			this._networkStream?.Dispose();
			this._tcpClient?.Dispose();

			this._logger.LogTrace($"Resources of connection to '{this.ServerInfo}' cleared.");

		}

		private void EnsureClientNotStarted()
		{
			if (this._tcpClient != null)
				throw new InvalidOperationException("Client has already been started!");
		}

		private void EnsureClientStarted([NotNull()] TcpClient? client)
		{
			if (client == null)
				throw new InvalidOperationException("Client has not been started!");
		}
	}
}

using Microsoft.Extensions.Logging;
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
		private readonly string _serverIpAddress;
		private readonly int _serverPort;

		private TcpClient? _tcpClient = null;
		private NetworkStream? _networkStream = null;
		
		public TCPNetworkClient(
			ILogger<TCPNetworkClient> logger,
			string serverIpAddress, 
			int serverPort)
		{
			this._logger = logger;
			this._serverIpAddress = serverIpAddress;
			this._serverPort = serverPort;
		}

		public async Task Start()
		{
			EnsureClientNotStarted();

			try
			{
				this._logger.LogInformation($"Staring client.");

				var client = new TcpClient();

				this._logger.LogInformation($"Trying to connect to '{_serverIpAddress}' on Port '{_serverPort}'");

				await client.ConnectAsync(_serverIpAddress, _serverPort);

				this._tcpClient = client;
				this._networkStream = client.GetStream();

				this._logger.LogInformation($"Client connected to '{_serverIpAddress}' on Port '{_serverPort}'");
			}
			catch (Exception ex)
			{
				this._logger.LogError(ex, $"Error when connection to '{_serverIpAddress}' on Port '{_serverPort}'");
			}
		}

		public Task Stop()
		{
			EnsureClientStarted(this._tcpClient);

			this._logger.LogInformation($"Closing connection with '{_serverIpAddress}' on Port '{_serverPort}'");

			this._tcpClient.Close();

			this._logger.LogInformation($"Connection closed with '{_serverIpAddress}' on Port '{_serverPort}'");

			return Task.CompletedTask;
		}

		public async Task SendMessage(string message)
		{
			EnsureClientStarted(this._tcpClient);

			this._logger.LogInformation($"Sending message '{message}' to '{_serverIpAddress}' on Port '{_serverPort}'");

			using (var serverStreamWriter = new StreamWriter(this._networkStream!))
			{
				await serverStreamWriter.WriteAsync(message);
				await serverStreamWriter.FlushAsync();
			}

			this._logger.LogInformation($"Message '{message}' send successfully to '{_serverIpAddress}' on Port '{_serverPort}'");
		}

		public async Task<string?> WaitForMessageFromServer()
		{
			EnsureClientStarted(this._tcpClient);

			this._logger.LogInformation($"Waiting for message from '{_serverIpAddress}' connected on Port '{_serverPort}'");

			using (var serverStreamReader = new StreamReader(this._networkStream!))
			{
				var message = await serverStreamReader.ReadLineAsync();

				this._logger.LogInformation($"Message from '{_serverIpAddress}' connected on Port '{_serverPort}' recieved");

				return message;
			}
		}


		public void Dispose()
		{
			this._logger.LogInformation($"Clearning resources of connection to '{_serverIpAddress}' connected on Port '{_serverPort}'");

			this._tcpClient?.Dispose();

			this._logger.LogInformation($"Resources of connection to '{_serverIpAddress}' connected on Port '{_serverPort}' cleared.");

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

			if (client.Connected == false)
				throw new InvalidOperationException("Client has not been connected!");
		}
	}
}

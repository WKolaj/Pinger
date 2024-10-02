using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pinger.Server.NetworkListening
{
    public class TCPNetworkListner : INetworkListner
    {
        private readonly ILogger<TCPNetworkListner> _logger;
        private int _port;
        private IPAddress _ipAddress;

        public TCPNetworkListner(ILogger<TCPNetworkListner> logger, int port) :
            this(logger, port, IPAddress.Any)
        {

        }

        public TCPNetworkListner(ILogger<TCPNetworkListner> logger, int port, IPAddress ipAddress)
        {
            _logger = logger;
            _port = port;
            _ipAddress = ipAddress;
        }

        public async Task StartListening(Func<string, StreamWriter, Task> onIncomingData, CancellationToken token)
        {
            using (var server = new TcpListener(_ipAddress, _port))
            {
                _logger.LogInformation($"Starting TCP listining on '{_ipAddress}' on Port '{_port}'");

                server.Start();

                _logger.LogInformation($"Listening TCP started on '{_ipAddress}' on Port '{_port}'");

                while (!token.IsCancellationRequested)
				{
					_logger.LogInformation($"Waiting for new clients to connect...");

					TcpClient client = await WaitForClientToConnect(server);

					_logger.LogInformation($"New client connected.");

					Task.Run(() => HandleClient(client, onIncomingData, token));
				}

				_logger.LogInformation($"Stopping TCP listining on '{_ipAddress}' on Port '{_port}'");

                server.Stop();

                _logger.LogInformation($"TCP listening stopped '{_ipAddress}' on Port '{_port}'");
            }

            _logger.LogInformation($"TCP server closed '{_ipAddress}' on Port '{_port}'");
        }

		private static async Task<TcpClient> WaitForClientToConnect(TcpListener server)
		{
			return await server.AcceptTcpClientAsync().ConfigureAwait(false);
		}

		private async Task HandleClient(TcpClient client, Func<string, StreamWriter, Task> onIncomingData, CancellationToken token)
        {
            try
            {
                var infoAboutClient = client.Client.RemoteEndPoint as IPEndPoint;

                _logger.LogInformation($"Connected with client '{infoAboutClient?.Address}' on Port '{infoAboutClient?.Port}'");

                using (var streamWithClient = client.GetStream())
                {
                    using (var clientStreamReader = new StreamReader(streamWithClient))
                    using (var clientStreamWriter = new StreamWriter(streamWithClient))
					{
						_logger.LogInformation($"Waiting for data form client '{infoAboutClient?.Address}' on Port {infoAboutClient?.Port}");

						while (!token.IsCancellationRequested && client.Connected)
						{
							var data = await WaitForNewDataFromTheClient(clientStreamReader);

							_logger.LogInformation($"New Data from client '{infoAboutClient?.Address}' on Port {infoAboutClient?.Port}");

							await onIncomingData(data, clientStreamWriter);

							_logger.LogInformation($"Waiting for data form client '{infoAboutClient?.Address}' on Port {infoAboutClient?.Port}");
						}

						_logger.LogInformation($"Closing connection with client '{infoAboutClient?.Address}' on Port '{infoAboutClient?.Port}'");
                    }
                }
            }
            finally
            {
                var infoAboutClient = client.Client.RemoteEndPoint as IPEndPoint;

                client.Dispose();

                _logger.LogInformation($"Connection closed with client '{infoAboutClient?.Address}' on Port '{infoAboutClient?.Port}'");
            }
        }

		private static async Task<string> WaitForNewDataFromTheClient(StreamReader clientStreamReader)
		{
			string? data = null;

			while (data == null)
			{
				data = await clientStreamReader.ReadLineAsync().ConfigureAwait(false);

				await Task.Delay(100);
			}

			return data;
		}
	}
}

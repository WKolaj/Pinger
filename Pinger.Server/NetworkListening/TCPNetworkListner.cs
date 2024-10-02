﻿using System;
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

                    var client = await server.AcceptTcpClientAsync().ConfigureAwait(false);

                    _logger.LogInformation($"New client connected.");

                    Task.Run(() => HandleClient(client, onIncomingData, token));
                }

                _logger.LogInformation($"Stopping TCP listining on '{_ipAddress}' on Port '{_port}'");

                server.Stop();

                _logger.LogInformation($"TCP listening stopped '{_ipAddress}' on Port '{_port}'");
            }

            _logger.LogInformation($"TCP server closed '{_ipAddress}' on Port '{_port}'");
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
                        while (!token.IsCancellationRequested)
                        {
                            _logger.LogInformation($"Waiting for data form client '{infoAboutClient?.Address}' on Port {infoAboutClient?.Port}");

                            var data = await clientStreamReader.ReadLineAsync().ConfigureAwait(false);

                            if (data != null)
                            {
                                _logger.LogInformation($"New Data from client '{infoAboutClient?.Address}' on Port {infoAboutClient?.Port}");

                                await onIncomingData(data, clientStreamWriter);
                            }
                            else
                            {
                                _logger.LogWarning($"Recieved empty data from {infoAboutClient?.Address}");
                            }
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
    }
}
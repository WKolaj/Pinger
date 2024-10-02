using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pinger.Common;
using Pinger.Common.Configurations;
using Pinger.Server.Networking;
using Pinger.Server.NetworkListening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server
{
	internal class PingServerBootstrapper
	{
		private IServiceProvider ServiceProvider = null!;
		private ILogger<PingServerBootstrapper> Logger = null!;

		const string TestMessageToClient = "Test Message To Client";

		public void ConfigureServices(IServiceCollection services)
		{
			var configProvider = services.AddConfiguration();

			services.AddLogging(
				options =>
					options
						.AddConfiguration(configProvider.Configuration.GetSection("Logging"))
						.AddConsole());

			services.AddScoped<INetworkListenerFactory, NetworkListenerFactory>();
		}

		public async Task Run(IServiceProvider serviceProvider, object[] args)
		{
			AssignServiceProvider(serviceProvider);
			AssignLogger(serviceProvider);

			var serverConfig = await GetServerConfig(serviceProvider);

			ValideServerConfig(serverConfig);

			var listener = CreateListener(serviceProvider, serverConfig);

			await StartListening(listener);
		}

		private async Task StartListening(INetworkListener listener)
		{
			await listener.StartListening(OnIncomingData, CancellationToken.None);

			this.Logger.LogInformation($"Started listening on address '{listener.Address}' and port '{listener.Port}'");
		}

		private INetworkListener CreateListener(IServiceProvider serviceProvider, ServerConfig serverConfig)
		{
			var factory = this.ServiceProvider.GetRequiredService<INetworkListenerFactory>();

			var listener = factory.CreateNetworkListener(serverConfig.Protocol, serverConfig.Port, serverConfig.IPAddress);

			return listener;
		}

		private void AssignServiceProvider(IServiceProvider serviceProvider)
		{
			this.ServiceProvider = serviceProvider;
		}

		private void AssignLogger(IServiceProvider serviceProvider)
		{
			this.Logger = serviceProvider.GetRequiredService<ILogger<PingServerBootstrapper>>();
		}

		private static async Task<ServerConfig> GetServerConfig(IServiceProvider serviceProvider)
		{
			var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

			var serverConfig = await configProvider.GetAppConfigAsync<ServerConfig>(ServerConfig.SectionName);

			return serverConfig;
		}

		private void ValideServerConfig(ServerConfig serverConfig)
		{
			Protocols.Validate(serverConfig.Protocol);
		}

		private async Task OnIncomingData(ClientInfo clientInfo, string incomingData, NetworkStream streamToClient)
		{
			this.Logger.LogInformation($"[{clientInfo}]:{incomingData}");

			this.Logger.LogInformation($"Sending message '{TestMessageToClient}' to client");

			var bytes = Encoding.UTF8.GetBytes(TestMessageToClient);

			await streamToClient.WriteAsync(bytes, 0, bytes.Length);
			await streamToClient.FlushAsync();
		}

	}
}

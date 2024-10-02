using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pinger.Common;
using Pinger.Common.Configurations;
using Pinger.Server.NetworkListening;
using System;
using System.Collections.Generic;
using System.Linq;
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
			services.AddLogging(
				options => options.AddConsole());

			services.AddConfiguration();

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

		private async Task StartListening(INetworkListener listner)
		{
			await listner.StartListening(OnIncomingData, CancellationToken.None);
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

		private async Task OnIncomingData(string incomingData, StreamWriter clientStreamWriter)
		{
			this.Logger.LogInformation(incomingData);

			this.Logger.LogInformation($"Sending message '{TestMessageToClient}' back to client");

			await clientStreamWriter.WriteLineAsync(incomingData);
			await clientStreamWriter.FlushAsync();

			this.Logger.LogInformation($"Message send back to client successfully!");
		}

	}
}

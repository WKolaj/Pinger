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

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				options => options.AddConsole());

			services.AddConfiguration();

			services.AddScoped<INetworkListnerFactory, NetworkListnerFactory>();
		}

		public async Task Run(IServiceProvider serviceProvider, object[] args)
		{
			AssignServiceProvider(serviceProvider);

			var serverConfig = await GetServerConfig(serviceProvider);

			ValideServerConfig(serverConfig);

			var listner = CreateListner(serviceProvider, serverConfig);

			await StartListening(listner);

		}

		private async Task StartListening(INetworkListner listner)
		{
			await listner.StartListening(OnIncomingData, CancellationToken.None);
		}

		private INetworkListner CreateListner(IServiceProvider serviceProvider, ServerConfig serverConfig)
		{
			var factory = this.ServiceProvider.GetRequiredService<INetworkListnerFactory>();

			var listner = factory.CreateNetworkListner(serverConfig.Protocol, serverConfig.Port, serverConfig.IPAddress);

			return listner;
		}

		private void AssignServiceProvider(IServiceProvider serviceProvider)
		{
			this.ServiceProvider = serviceProvider;
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

		private Task OnIncomingData(string incomingData, StreamWriter clientStreamWriter)
		{
            Console.WriteLine(incomingData);

			return Task.CompletedTask;
		}

	}
}

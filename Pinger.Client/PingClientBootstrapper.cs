using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pinger.Client.NetworkListening;
using Pinger.Common;
using Pinger.Common.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Client
{
	internal class PingClientBootstrapper
	{
		private IServiceProvider ServiceProvider = null!;
		private ILogger<PingClientBootstrapper> Logger = null!;

		private const string MessageToServerContent = "Test Message To Server";

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				options => options.AddConsole());

			services.AddConfiguration();

			services.AddScoped<INetworkClientFactory, NetworkClientFactory>();
		}

		public async Task Run(IServiceProvider serviceProvider, object[] args)
		{
			AssignServiceProvider(serviceProvider);
			AssignLogger(serviceProvider);

			var clientConfig = await GetClientConfig(serviceProvider);

			ValideClientConfig(clientConfig);

			using(var client = CreateClient(serviceProvider, clientConfig))
			{
				await client.Start();

				while (true)
				{
					await SendMessageToServer(client, MessageToServerContent);

					var message = await GetMessageFromServer(client);

					this.Logger.LogInformation(message);

					await WaitMiliseconds(1000);
				}
			}
		}

		private void AssignServiceProvider(IServiceProvider serviceProvider)
		{
			this.ServiceProvider = serviceProvider;
		}

		private void AssignLogger(IServiceProvider serviceProvider)
		{
			this.Logger = serviceProvider.GetRequiredService<ILogger<PingClientBootstrapper>>();
		}

		private static async Task<ClientConfig> GetClientConfig(IServiceProvider serviceProvider)
		{
			var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

			var clientConfig = await configProvider.GetAppConfigAsync<ClientConfig>(ClientConfig.SectionName);

			return clientConfig;
		}

		private void ValideClientConfig(ClientConfig clientConfig)
		{
			Protocols.Validate(clientConfig.Protocol);
		}

		private INetworkClient CreateClient(IServiceProvider serviceProvider, ClientConfig clientConfig)
		{
			var factory = this.ServiceProvider.GetRequiredService<INetworkClientFactory>();

			var client = factory.CreateNetworkClient(clientConfig.Protocol, clientConfig.ServerAddress, clientConfig.ServerPort);

			return client;
		}

		private async Task SendMessageToServer(INetworkClient client, string message)
		{
			await client.SendMessage(message);
		}

		private async Task<string?> GetMessageFromServer(INetworkClient client)
		{
			return await client.WaitForMessageFromServer();
		}

		private static async Task WaitMiliseconds(int miliseconds)
		{
			await Task.Delay(miliseconds);
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
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

		private const string MessageToServerContent = "Test Message From Client";

		public void ConfigureServices(IServiceCollection services)
		{
			var configProvider = services.AddConfiguration();

			services.AddLogging(
				options =>
					options
						.AddConfiguration(configProvider.Configuration.GetSection("Logging"))
						.AddConsole());

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

				this.Logger.LogInformation($"Starting communcation with server '{client.ServerInfo}'");

				while (true)
				{
					await SendMessageToServer(client, MessageToServerContent);

					var message = await GetMessageFromServer(client);

					this.Logger.LogInformation($"Response from server '{client.ServerInfo}': {message}");

					await WaitMiliseconds(clientConfig.TimeBetweenSendingMessages);
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
			try
			{
				this.Logger.LogInformation($"Sending message '{message}' to server '{client.ServerInfo}'");

				await client.SendMessage(message);
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Exception during sending message to client");
			}
		}

		private async Task<string?> GetMessageFromServer(INetworkClient client)
		{
			try
			{
				this.Logger.LogInformation($"Waiting for response from server '{client.ServerInfo}'");

				return await client.WaitForMessageFromServer();
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Exception during reading message from server");

				return null;
			}
		}

		private static async Task WaitMiliseconds(int miliseconds)
		{
			await Task.Delay(miliseconds);
		}
	}
}

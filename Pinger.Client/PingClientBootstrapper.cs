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

			OverwriteConfigWithCmdArguments(clientConfig, args);

			ValideClientConfig(clientConfig);

			using(var client = CreateClient(serviceProvider, clientConfig))
			{
				this.Logger.LogInformation($"Trying to connect to server: '{client.ServerInfo}'");

				await client.Start();

				this.Logger.LogInformation($"Connection to server established: '{client.ServerInfo}'");

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

		private void OverwriteConfigWithCmdArguments(ClientConfig clientConfig, object[] args)
		{
			if (args.Length <= 0)
				return;

			if(args.Length > 0)
			{
				var arg0 = args[0].ToString();

				if (String.IsNullOrWhiteSpace(arg0) == false)
				{
					if (TextContainsIpAndPort(arg0))
					{
						clientConfig.ServerAddress = GetIp(arg0);
						clientConfig.ServerPort = GetPort(arg0);
					}
					else
					{
						clientConfig.ServerAddress = arg0;
					}
				}
			}

			if (args.Length > 1)
			{
				var arg1 = args[1].ToString();

				if (String.IsNullOrWhiteSpace(arg1) == false)
				{
					clientConfig.TimeBetweenSendingMessages = Int32.Parse(arg1);
				}
			}

			if (args.Length > 2)
			{
				var arg2 = args[2].ToString();

				if (String.IsNullOrWhiteSpace(arg2) == false)
				{
					clientConfig.Protocol = arg2;
				}
			}
		}

		private bool TextContainsIpAndPort(string argument)
		{
			return argument.Contains(":");
		}

		private string GetIp(string argument)
		{
			return argument.Split(":").First();
		}

		private int GetPort(string argument)
		{
			var portText = argument.Split(":").Skip(1).First();

			return Int32.Parse(portText);
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

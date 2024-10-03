using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pinger.Common;
using Pinger.Common.Configurations;
using Pinger.Common.Networking;
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

		const string TestMessageToClient = "Test Message From Server";

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

			OverwriteConfigWithCmdArguments(serverConfig, args);

			ValideServerConfig(serverConfig);

			var listener = CreateListener(serviceProvider, serverConfig);

			await StartListening(listener);
		}

		private async Task StartListening(INetworkListener listener)
		{
			this.Logger.LogInformation($"Starting listening on address '{new ClientInfo(listener.Address.ToString(), listener.Port)}'");

			await listener.StartListening(OnIncomingData, CancellationToken.None);
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

		private void OverwriteConfigWithCmdArguments(ServerConfig serverConfig, object[] args)
		{
			if (args.Length <= 0)
				return;

			if (args.Length > 0)
			{
				var arg0 = args[0].ToString();

				if (String.IsNullOrWhiteSpace(arg0) == false)
				{
					serverConfig.Port = Int32.Parse(arg0);
				}
			}

			if (args.Length > 1)
			{
				var arg1 = args[1].ToString();

				if (String.IsNullOrWhiteSpace(arg1) == false)
				{
					serverConfig.IPAddress = arg1;
				}
			}

			if (args.Length > 2)
			{
				var arg2 = args[2].ToString();

				if (String.IsNullOrWhiteSpace(arg2) == false)
				{
					serverConfig.Protocol = arg2;
				}
			}
		}

		private void ValideServerConfig(ServerConfig serverConfig)
		{
			Protocols.Validate(serverConfig.Protocol);
		}

		private async Task OnIncomingData(ClientInfo clientInfo, string incomingData, NetworkStream streamToClient)
		{
			this.Logger.LogInformation($"Message from client '{clientInfo}': '{incomingData}'");

			try
			{
				this.Logger.LogInformation($"Sending message back to client: '{TestMessageToClient}'");

				await streamToClient.FlushMessageToStream(TestMessageToClient);
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Exception during sending message to client");
			}
		}

	}
}

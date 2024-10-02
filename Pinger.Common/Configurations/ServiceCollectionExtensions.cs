using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Common.Configurations
{
	public static class ServiceCollectionExtensions
	{
		public static IConfigProvider AddConfiguration(this IServiceCollection serviceCollection)
		{
			var configurationProvider = new ConfigProvider();

			serviceCollection.AddSingleton<IConfigProvider>(configurationProvider);

			return configurationProvider;
		}
	}
}

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
		public static void AddConfiguration(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddScoped<IConfigProvider, ConfigProvider>();
		}
	}
}

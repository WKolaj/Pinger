using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Pinger.Common.Configurations
{
	internal class ConfigProvider : IConfigProvider
	{
		public IConfiguration Configuration { get; }

		private const string appSettingsFileName = "appsettings.json";

		public ConfigProvider()
		{
			var builder = new ConfigurationBuilder();

			builder.SetBasePath(Directory.GetCurrentDirectory())
				   .AddJsonFile(appSettingsFileName, optional: false, reloadOnChange: true);

			this.Configuration = builder.Build();
		}

		public Task<TSection> GetAppConfigAsync<TSection>(string? sectionName = null)
		{
			IConfiguration config = Configuration;

			var nameOfConfigSection = sectionName ?? typeof(TSection).Name;

			var section = config.GetRequiredSection(nameOfConfigSection).Get<TSection>();

			if (section == null)
				throw new ArgumentException($"There is not '{nameOfConfigSection}' config section in config file!");

			return Task.FromResult(section);
		}
	}
}

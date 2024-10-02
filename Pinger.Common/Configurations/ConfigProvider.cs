using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Common.Configurations
{
	internal class ConfigProvider : IConfigProvider
	{
		private const string appSettingsFileName = "appsettings.json";

		public Task<TSection> GetAppConfigAsync<TSection>(string? sectionName = null)
		{
			var builder = new ConfigurationBuilder();

			builder.SetBasePath(Directory.GetCurrentDirectory())
				   .AddJsonFile(appSettingsFileName, optional: false, reloadOnChange: true);

			var nameOfConfigSection = sectionName ?? typeof(TSection).Name;

			IConfiguration config = builder.Build();

			var section = config.GetRequiredSection(nameOfConfigSection).Get<TSection>();

			if (section == null)
				throw new ArgumentException($"There is not '{nameOfConfigSection}' config section in config file!");

			return Task.FromResult(section);
		}
	}
}

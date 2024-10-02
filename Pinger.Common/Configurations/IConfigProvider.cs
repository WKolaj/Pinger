using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Pinger.Common.Configurations
{
	public interface IConfigProvider
	{
		Task<TSection> GetAppConfigAsync<TSection>(string? sectionName = null);
	}
}

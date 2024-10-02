using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Server.NetworkListening
{
    public interface INetworkListener
	{
        Task StartListening(Func<string, StreamWriter, Task> onIncomingData, CancellationToken token);
    }
}

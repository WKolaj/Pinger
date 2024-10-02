using Microsoft.Extensions.DependencyInjection;
using Pinger.Client;
using System.Net.Sockets;
using System.Text;

IServiceCollection services = new ServiceCollection();

var bootstrapper = new PingClientBootstrapper();

bootstrapper.ConfigureServices(services);

IServiceProvider serviceProvider = services.BuildServiceProvider();

await bootstrapper.Run(serviceProvider, args);
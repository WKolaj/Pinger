using Microsoft.Extensions.DependencyInjection;
using Pinger.Server;

IServiceCollection services = new ServiceCollection();

var bootstrapper = new PingServerBootstrapper();

bootstrapper.ConfigureServices(services);

IServiceProvider serviceProvider = services.BuildServiceProvider();

await bootstrapper.Run(serviceProvider, args);

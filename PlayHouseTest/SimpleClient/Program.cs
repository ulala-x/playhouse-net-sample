using Serilog;

namespace SimpleClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .WriteTo.Console()
               .CreateLogger();

            var client = new ClientApplication();
            await client.RunAsync();
        }
    }
}
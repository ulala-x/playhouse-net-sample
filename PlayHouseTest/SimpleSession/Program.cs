
using Serilog;

namespace SimpleSession
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                   .WriteTo.Console()
                   .CreateLogger();

            var runner = new SessionApplication();
            runner.Run();
        }
    }
}
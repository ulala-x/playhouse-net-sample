using Serilog;
using SimplePlay;

namespace SimplePlay
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                  .WriteTo.Console()
                  .CreateLogger();

            var runner = new PlayApplication();
            runner.Run();
        }
    }
}
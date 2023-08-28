using Serilog;

namespace SimpleApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            
             Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

            var runner = new ApiApplication();
            runner.RegisterService();
            runner.Run();
             
        }
    }
}
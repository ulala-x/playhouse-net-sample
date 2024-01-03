using PlayHouse.Utils;
using Serilog;
using SimpleConfigure;

namespace SimpleClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string logFilePath = $"logs/simple.txt";

            // 로그 파일이 이미 존재하면 삭제
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            LoggerConfigure.SetLogger(new SimpleLogger(),LogLevel.Trace);

            // Serilog 구성
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug) // 콘솔에는 정보 레벨 이상만 로그
                         .WriteTo.Async(a => a.File(logFilePath,shared:true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose))
                        .CreateLogger();

            Task[] tasks = new Task[1];
            

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run( async () =>
                {
                    var client = new ClientApplication();
                    await client.RunAsync();
                });
            }

            await Task.WhenAll(tasks);
            //Environment.Exit(0);

            // var client = new ClientApplication();
            // await client.RunAsync();
        }
    }
}
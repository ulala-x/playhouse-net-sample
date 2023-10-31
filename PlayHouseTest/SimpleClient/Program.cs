using Microsoft.Extensions.Logging;
using Serilog;

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


            // Serilog 구성
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information) // 콘솔에는 정보 레벨 이상만 로그
                         .WriteTo.Async(a => a.File(logFilePath,shared:true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)) // 파일에는 디버그 레벨 이상 로그
                        .CreateLogger();

            Task[] tasks = new Task[100];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run( async () =>
                {
                    var client = new ClientApplication();
                    await client.RunAsync();
                });
            }

            await Task.WhenAll(tasks);
            Environment.Exit(0);

            // var client = new ClientApplication();
            // await client.RunAsync();
        }
    }
}
using PlayHouse.Utils;
using Serilog;
using Serilog.Events;
using SimpleConfigure;

namespace SimpleStress;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var logFilePath = "logs/simple.txt";

        // 로그 파일이 이미 존재하면 삭제
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        LoggerConfigure.SetLogger(new SimpleLogger(), LogLevel.Info);

        // Serilog 구성
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Debug) // 콘솔에는 정보 레벨 이상만 로그
            .WriteTo.Async(a => a.File(logFilePath, shared: true, restrictedToMinimumLevel: LogEventLevel.Verbose))
            .CreateLogger();

        var tasks = new Task[1000];


        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(async () =>
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
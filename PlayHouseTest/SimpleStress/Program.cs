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

        LoggerConfigure.SetLogger(new SimpleLogger(), LogLevel.Debug);

        // Serilog 구성
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Verbose) // 콘솔에는 정보 레벨 이상만 로그
            .WriteTo.Async(a => a.File(logFilePath, shared: true, restrictedToMinimumLevel: LogEventLevel.Verbose))
            .CreateLogger();


        int count = 1000;
        List<ClientApplication> applications = new List<ClientApplication>();

        for (int i = 0; i < count; ++i)
        {
            applications.Add(new ClientApplication());
        }

        var prePareTask = new Task[count];
        for (var i = 0; i < count; i++)
        {
            int index  = i;
            prePareTask[i] = Task.Run(async () =>
            {
                await applications[index].PrePareAsync();
            });
        }

        await Task.WhenAll(prePareTask);


        var runTask = new Task[count];
        for (var i = 0; i < count; i++)
        {
            int index = i;
            runTask[i] = Task.Run(async () =>
            {
                await applications[index].RunAsync();
            });
        }

        await Task.WhenAll(runTask);

        // var client = new ClientApplication();
        // await client.RunAsync();
    }
}
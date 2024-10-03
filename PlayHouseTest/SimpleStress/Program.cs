using System.Diagnostics;
using PlayHouse.Utils;
using Serilog;
using Serilog.Events;
using SimpleConfigure;

namespace SimpleStress;


internal class Program
{
    private static void Main(string[] args)
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
            .WriteTo.Console(LogEventLevel.Verbose) // 콘솔에는 정보 레벨 이상만 로그
            .WriteTo.Async(a => a.File(logFilePath, shared: true, restrictedToMinimumLevel: LogEventLevel.Verbose))
            .CreateLogger();


        Stopwatch sw = Stopwatch.StartNew();
        Log.Logger.Information("Start");
        var application = new StressApplication(5000);
        application.Connect();
        application.Prepare();
        application.Run();
        sw.Stop();
        Log.Logger.Information($"End - elapsed:{sw.ElapsedMilliseconds}");

        //Thread.Sleep(1000);
        //Environment.Exit(0);


        // var client = new TestClient();
        // await client.RunAsync();
    }
}
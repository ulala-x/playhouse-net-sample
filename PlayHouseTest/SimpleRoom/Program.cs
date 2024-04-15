using PlayHouse.Utils;
using Serilog;
using SimpleConfigure;

namespace SimplePlay;

internal class Program
{
    static void Main(string[] args)
    {
        string logFilePath = $"logs/simple.txt";

        // 로그 파일이 이미 존재하면 삭제
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }
        LoggerConfigure.SetLogger(new SimpleLogger(),LogLevel.Debug);
        // Serilog 구성
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose) 
            .WriteTo.Async(a => a.File(logFilePath, shared: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose))
            .CreateLogger();

        var runner = new PlayApplication();
        runner.Run();
    }
}
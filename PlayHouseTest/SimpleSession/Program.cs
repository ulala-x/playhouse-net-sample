using PlayHouse.Utils;
using Serilog;
using Serilog.Events;
using SimpleConfigure;

namespace SimpleSession;

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
            .WriteTo.Console(LogEventLevel.Verbose)
            .WriteTo.Async(a => a.File(logFilePath, shared: true, restrictedToMinimumLevel: LogEventLevel.Verbose))
            .CreateLogger();

        var runner = new SessionApplication();
        runner.Run();
    }
}
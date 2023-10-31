using Serilog;
using SimplePlay;

namespace SimplePlay
{
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


            // Serilog 구성
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information) // 콘솔에는 정보 레벨 이상만 로그
                         .WriteTo.Async(a => a.File(logFilePath, shared: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)) // 파일에는 디버그 레벨 이상 로그
                        .CreateLogger();

            var runner = new PlayApplication();
            runner.Run();
        }
    }
}
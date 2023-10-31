using PlayHouse.Production;
using Serilog;

namespace SimpleConfigure
{
    public class SimpleLogger : IPlayHouseLogger
    {
        public void Debug(Func<string> messageFactory, string className)
        {
            Log.Logger.Debug($"{className}:{messageFactory()}");
        }

        public void Error(Func<string> messageFactory, string className)
        {
            Log.Logger.Error($"{className}:{messageFactory()}");
        }

        public void Fatal(Func<string> messageFactory, string className)
        {
            Log.Logger.Fatal($"{className}:{messageFactory()}");
        }

        public void Info(Func<string> messageFactory, string className)
        {
            Log.Logger.Information($"{className}:{messageFactory()}");
        }

        public void Trace(Func<string> messageFactory, string className)
        {
            Log.Logger.Verbose($"{className}:{messageFactory()}");
        }

        public void Warn(Func<string> messageFactory, string className)
        {
            Log.Logger.Warning($"{className}:{messageFactory()}");
        }
    }
}
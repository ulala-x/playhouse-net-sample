using PlayHouse.Utils;
using Serilog;

namespace SimpleConfigure;

public class SimpleLogger : IPlayHouseLogger
{
    public void Debug(Func<string> messageFactory, string className, string methodName)
    {
        Log.Logger.Debug($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Error(Func<string> messageFactory, string className, string methodName)
    {
        Log.Logger.Error($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Fatal(Func<string> messageFactory, string className, string methodName)
    {
        Log.Logger.Fatal($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Info(Func<string> messageFactory, string className, string methodName)
    {
        Log.Logger.Information($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Trace(Func<string> messageFactory, string className, string methodName)
    {
        Log.Logger.Verbose($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Warn(Func<string> messageFactory, string className, string methodName)
    {
        Log.Logger.Warning($"[{className}] ({methodName}) {messageFactory()}");
    }
}
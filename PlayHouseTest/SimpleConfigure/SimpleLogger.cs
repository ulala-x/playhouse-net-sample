using PlayHouse.Utils;
using Serilog;

namespace SimpleConfigure;

public class SimpleLogger : IPlayHouseLogger
{

    public void Debug(Func<FormattableString> messageFactory, string className, string methodName)
    {
        Log.Logger.InterpolatedDebug($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Info(Func<FormattableString> messageFactory, string className, string methodName)
    {
        Log.Logger.InterpolatedInformation($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Warn(Func<FormattableString> messageFactory, string className, string methodName)
    {
        Log.Logger.InterpolatedWarning($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Error(Func<FormattableString> messageFactory, string className, string methodName)
    {
        Log.Logger.InterpolatedError($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Trace(Func<FormattableString> messageFactory, string className, string methodName)
    {
        Log.Logger.InterpolatedVerbose($"[{className}] ({methodName}) {messageFactory()}");
    }

    public void Fatal(Func<FormattableString> messageFactory, string className, string methodName)
    {
        Log.Logger.InterpolatedFatal($"[{className}] ({methodName}) {messageFactory()}");
    }
}
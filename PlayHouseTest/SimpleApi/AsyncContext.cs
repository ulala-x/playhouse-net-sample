using PlayHouse.Production.Shared;

namespace SimpleApi;

public class AsyncContext
{
    private static readonly AsyncLocal<IApiSender?> _apiSenderContext = new();
    private static readonly AsyncLocal<ErrorCodeWrapper?> _errorCode = new();

    public static IApiSender? ApiSender
    {
        get => _apiSenderContext.Value;
        set => _apiSenderContext.Value = value;
    }

    public static ushort ErrorCode
    {
        get => _errorCode.Value!.Code;
        set => _errorCode.Value!.Code = value;
    }

    public static void InitErrorCode()
    {
        _errorCode.Value = new ErrorCodeWrapper();
    }


    internal static void Clear()
    {
        _apiSenderContext.Value = null;
        _errorCode.Value = null;
    }

    private class ErrorCodeWrapper
    {
        public ushort Code { get; set; }
    }
}
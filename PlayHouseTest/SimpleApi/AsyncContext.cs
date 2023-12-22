using PlayHouse.Production;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi;

public class AsyncContext
{
    private class ErrorCodeWrapper
    {
        public ushort Code { get; set; }
    }

    private static readonly AsyncLocal<IApiSender?> _apiSenderContext = new();
    private static readonly AsyncLocal<ErrorCodeWrapper?> _errorCode = new();

    public static void InitErrorCode()
    {
        _errorCode.Value = new ErrorCodeWrapper();
    }

    public static IApiSender? ApiSender
    {
        get => _apiSenderContext.Value;
        set => _apiSenderContext.Value = value;
    }

    public static ushort ErrorCode
    {
        get
        {
            return _errorCode.Value!.Code;
        }
        set
        {
            _errorCode.Value!.Code = value;
        }
    }


    internal static void Clear()
    {
        _apiSenderContext.Value = null;
        _errorCode.Value = null;
    }
}

using System.Collections.Concurrent;
using PlayHouse.Production.Api;
using PlayHouse.Production.Shared;
using PlayHouse.Utils;
using Simple;
using SimpleProtocol;

namespace SimpleApi.System;

public class SimpleApiSystem : ISystemController, IUpdateServerInfoCallback
{
    private static readonly ConcurrentDictionary<string, IServerInfo> _serverInfos = new();
    private readonly LOG<SimpleApiSystem> _log = new();


    public void Handles(ISystemHandlerRegister handlerRegister)
    {
        handlerRegister.Add(HelloReq.Descriptor.Index, Hello);
    }

    public async Task<List<IServerInfo>> UpdateServerInfoAsync(IServerInfo serverInfo)
    {
        _serverInfos.AddOrUpdate(serverInfo.GetBindEndpoint(), endpoint => new ServerInfo(serverInfo),
            (endpoint, exist) => new ServerInfo(serverInfo));
        await Task.CompletedTask;

        return _serverInfos.Values.ToList();
    }


    public async Task Hello(IPacket packet, ISystemPanel panel, ISender sender)
    {
        _log.Info(() => $"{packet.MsgId} packet received");

        try
        {
            if (packet.MsgId == HelloReq.Descriptor.Index)
            {
                var message = packet.Parse<HelloReq>().Message;
                sender.Reply(new SimplePacket(new HelloRes { Message = message }));
                await Task.CompletedTask;
            }
        }
        catch (Exception e)
        {
            _log.Error(() => "exception message:" + e.Message);
            _log.Error(() => "exception trace:" + e.StackTrace);

            if (e.InnerException != null)
            {
                _log.Error(() => "internal exception message:" + e.InnerException.Message);
                _log.Error(() => "internal exception trace:" + e.InnerException.StackTrace);
            }
        }
    }
}
using PlayHouse.Communicator;
using PlayHouse.Production.Shared;
using PlayHouse.Utils;
using Simple;
using SimpleProtocol;

namespace SimpleApi.System;

public class SimpleApiSystem : ISystemController 
{
    private readonly LOG<SimpleApiSystem> _log = new();

    public void Handles(ISystemHandlerRegister handlerRegister)
    {
        handlerRegister.Add(HelloReq.Descriptor.Name, Hello);
    }


    public async Task<IReadOnlyList<IServerInfo>> UpdateServerInfoAsync(IServerInfo serverInfo)
    {

        List<ServerInfo> servers =
        [
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = serverInfo.GetBindEndpoint(),
                Nid = serverInfo.GetNid(),
                ServiceType = serverInfo.GetServiceType().ToString(),
                ServcieId = serverInfo.GetServiceId(),
                State = serverInfo.GetState().ToString(),
                LastUpdate = serverInfo.GetLastUpdate(),
                ActorCount = serverInfo.GetActorCount()
            }),
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = "tcp://127.0.0.1:10370",
                Nid = 1,
                ServiceType = ServiceType.SESSION.ToString(),
                ServcieId = (int)ServiceId.Session,
                State = ServerState.RUNNING.ToString(),
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ActorCount = 0
            }),
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = "tcp://127.0.0.1:10570",
                Nid = 3,
                ServiceType = ServiceType.Play.ToString(),
                ServcieId = (int)ServiceId.Play,
                State = ServerState.RUNNING.ToString(),
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ActorCount = 0
            })

        ];
        await Task.CompletedTask;
        return servers;

    }


    public async Task Hello(IPacket packet, ISystemPanel panel, ISender sender)
    {
        _log.Info(() => $"{packet.MsgId} packet received");

        try
        {
            if (packet.MsgId == HelloReq.Descriptor.Name)
            {
                var message = packet.Parse<HelloReq>().Message;
                sender.Reply(new SimplePacket(new HelloRes { Message = message }));
                await Task.CompletedTask;
            }
        }
        catch (Exception e)
        {
            _log.Error(() => $"exception message:{e.Message}" );
            _log.Error(() => $"exception trace:{e.StackTrace}");

            if (e.InnerException != null)
            {
                _log.Error(() => $"internal exception message:{e.InnerException.Message}");
                _log.Error(() => $"internal exception trace:{e.InnerException.StackTrace}");
            }
        }
    }
}
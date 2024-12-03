using PlayHouse.Communicator;
using PlayHouse.Production.Shared;
using Simple;
using SimpleProtocol;

namespace SimpleSession;

public class SimpleSessionSystem : ISystemController
{
    public void Handles(ISystemHandlerRegister handlerRegister)
    {
    }

    public async Task<IReadOnlyList<IServerInfo>> UpdateServerInfoAsync(IServerInfo serverInfo)
    {

        List<ServerInfo> servers =
        [
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = serverInfo.GetBindEndpoint(),
                ServcieId = serverInfo.GetServiceId(),
                ServerId = serverInfo.GetServerId(),
                Nid = serverInfo.GetNid(),
                ServiceType = serverInfo.GetServiceType().ToString(),
                State = serverInfo.GetState().ToString(),
                LastUpdate = serverInfo.GetLastUpdate(),
                ActorCount = serverInfo.GetActorCount()
            }),
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = "tcp://127.0.0.1:10470",
                ServcieId = (int)ServiceId.Api,
                ServerId = 0,
                Nid = $"{ServiceId.Api.GetHashCode()}:0",
                ServiceType = ServiceType.API.ToString(),
                State = ServerState.RUNNING.ToString(),
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ActorCount = 0
            }),
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = "tcp://127.0.0.1:10570",
                ServcieId = (int)ServiceId.Play,
                ServerId = 0,
                Nid = $"{ServiceId.Play.GetHashCode()}:0",
                ServiceType = ServiceType.Play.ToString(),
                State = ServerState.RUNNING.ToString(),
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ActorCount = 0
            }),
        ];

        await Task.CompletedTask;
        return servers;

        //var api =  ControlContext.SystemPanel.GetServerInfoBy((int)ServiceId.Api);
        //string endpoint = "127.0.0.1:10407";

        //var res = await ControlContext.Sender.RequestToSystem(endpoint, new SimplePacket(new ServerInfoReq()));

        //var serverInfoRes = res.Parse<ServerInfoRes>();
        //return serverInfoRes.ServerInfos.Select(e => new ServerInfo(e)).ToList();
    }
}
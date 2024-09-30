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
                ServiceType = serverInfo.GetServiceType().ToString(),
                ServcieId = serverInfo.GetServiceId(),
                State = serverInfo.GetState().ToString(),
                LastUpdate = serverInfo.GetLastUpdate(),
                ActorCount = serverInfo.GetActorCount()
            }),
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = "tcp://127.0.0.1:10470",
                ServiceType = ServiceType.API.ToString(),
                ServcieId = (int)ServiceId.Api,
                State = ServerState.RUNNING.ToString(),
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ActorCount = 0
            }),
            new ServerInfo(new ServerInfoProto
            {
                BindEndpoint = "tcp://127.0.0.1:10570",
                ServiceType = ServiceType.Play.ToString(),
                ServcieId = (int)ServiceId.Play,
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
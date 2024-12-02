using PlayHouse.Communicator;
using PlayHouse.Production.Shared;
using Simple;

namespace SimpleProtocol;

public class ServerInfo(ServerInfoProto serverInfo) : IServerInfo
{
    public string GetBindEndpoint()
    {
        return serverInfo.BindEndpoint;
    }

    public int GetNid()
    {
        return serverInfo.Nid;
    }

    public ServiceType GetServiceType()
    {
        return Enum.Parse<ServiceType>(serverInfo.ServiceType, true);
    }

    public ushort GetServiceId()
    {
        return (ushort)serverInfo.ServcieId;
    }

    public ServerState GetState()
    {
        return Enum.Parse<ServerState>(serverInfo.State, true);
    }

    public long GetLastUpdate()
    {
        return serverInfo.LastUpdate;
    }

    public int GetActorCount()
    {
        return serverInfo.ActorCount;
    }
}
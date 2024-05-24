using PlayHouse.Communicator;
using PlayHouse.Production.Shared;

namespace SimpleApi.System;

internal class ServerInfo : IServerInfo
{
    private readonly int _actorCount;
    private readonly string _bindEndpoint = string.Empty;
    private readonly long _lastUpdate;
    private readonly ushort _serviceId;
    private readonly ServiceType _serviceType;
    private readonly ServerState _state;

    public ServerInfo(IServerInfo serverInfo)
    {
        _bindEndpoint = serverInfo.GetBindEndpoint();
        _serviceType = serverInfo.GetServiceType();
        _serviceId = serverInfo.GetServiceId();
        _state = serverInfo.GetState();
        _lastUpdate = serverInfo.GetLastUpdate();
        _actorCount = serverInfo.GetActorCount();
    }

    public int GetActorCount()
    {
        return _actorCount;
    }

    public string GetBindEndpoint()
    {
        return _bindEndpoint;
    }

    public long GetLastUpdate()
    {
        return _lastUpdate;
    }

    public ushort GetServiceId()
    {
        return _serviceId;
    }

    public ServiceType GetServiceType()
    {
        return _serviceType;
    }

    public ServerState GetState()
    {
        return _state;
    }
}
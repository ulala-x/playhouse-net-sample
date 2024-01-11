using PlayHouse.Communicator;
using PlayHouse.Production.Shared;


namespace SimpleApi.System
{
    internal class ServerInfo : IServerInfo
    {
        private readonly string _bindEndpoint = string.Empty;
        private readonly ServiceType _serviceType;
        private readonly ushort _serviceId;
        private readonly ServerState _state;
        private readonly long _lastUpdate;
        private readonly int _actorCount;

        public ServerInfo(IServerInfo serverInfo)
        {
            _bindEndpoint = serverInfo.BindEndpoint;
            _serviceType = serverInfo.ServiceType;
            _serviceId = serverInfo.ServiceId;
            _state = serverInfo.State;
            _lastUpdate = serverInfo.LastUpdate;
            _actorCount = serverInfo.ActorCount;
        }

        public string BindEndpoint => _bindEndpoint;

        public ServiceType ServiceType => _serviceType;

        public ushort ServiceId => _serviceId;

        public ServerState State => _state;

        public long LastUpdate => _lastUpdate;

        public int ActorCount => _actorCount;
    }
}

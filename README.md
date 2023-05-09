# playhouse-net-sample
PlayHouse 의 간략한 사용 예제 입니다. 
Client connector 를 통해서 API 서버로 메시지를 보내고 , Play Server 에 방 생성및 입장을 하는 예제입니다.

# How to Build
```
git clone https://github.com/ulala-x/playhouse-net-sample.git

서버간 주소 동기화를 위해서 redis 가 필요합니다.

docker run -d --name redis-simple-server -p 6379:6379 redis/redis-stack-server:latest
```

# Examples

## Simple Client
- Client Connector 를 이용해서 Session 서버에 접속하고 패킷을 보낸다.

## Simple Session
공통적으로 모든 서버는 commonOption 과 각 서버별 특성을 가진 Option 두가지를 가지고 옵션 설정이 가능하다.
```c#
 class SessionApplication
    {
        private readonly ILogger _log = Log.Logger;

        public void Run()
        {
            try
            {
                const int redisPort = 6379;

                short sessionSvcId = 1;
                short apiSvcId = 2;


                var commonOption = new CommonOption
                {
                    Port = 30370, // bind 할 서버 주소
                    ServiceId = sessionSvcId, // Service Id 
                    RedisPort = redisPort, // 주소 동기화를 위한 redis port
                    ServerSystem = (systemPanel, sender) => new SessionSystem(systemPanel, sender),
                    RequestTimeoutSec = 0, //request message 에 대한 time out 설정
                };

                var sessionOption = new SessionOption
                {
                    SessionPort = 30114, // 클리언트가 접속할 server port
                    ClientSessionIdleTimeout = 0, //client idle timeout
                    UseWebSocket = false, // 웹소켓 사용 여부
                    Urls = new List<string> { $"{apiSvcId}:{AuthenticateReq.Descriptor.Index}" } 
                    // 인증 패킷 할당. 인증을 처리할 service 와 패킷을 지정한다. 
                };

                var sessionServer = new SessionServer(commonOption, sessionOption);
                sessionServer.Start();

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    _log.Information("*** shutting down Session server since process is shutting down");
                    sessionServer.Stop();
                    _log.Information("*** server shut down");
                    Thread.Sleep(1000);
                };

                _log.Information("Session Server Started");
                sessionServer.AwaitTermination();
            }
            catch (Exception e)
            {
                _log.Error(e.StackTrace!);
                Environment.Exit(1);
            }
        }
    }
```
  모든 서버는 IServerSystem 인터페이스를 구현해야 하는데 이를 통해서 서버의 운영 시스템을 구축할 수 있다. 
``` c#
class SessionSystem : IServerSystem
    {
        ISystemPanel _panel;
        ISender _sender;
        private readonly ILogger _log = Log.Logger;

        public SessionSystem(ISystemPanel panel, ISender sender)
        {
            _panel = panel;
            _sender = sender;
        }

        public async Task OnDispatch(Packet packet)
        {
            _log.Information("OnDispatch");
            await Task.CompletedTask;
        }

        public async Task OnPause()
        {
            _log.Information("OnPause");
            await Task.CompletedTask;
        }

        public async Task OnResume()
        {
            _log.Information("OnResume");
            await Task.CompletedTask;
        }

        public async Task OnStart()
        {
            _log.Information("OnStart");
            await Task.CompletedTask;
        }

        public async Task OnStop()
        {
            _log.Information("OnStop");
            await Task.CompletedTask;
        }
    }
```

## Simple API
API 서버의 구현 기본 IServerSystem 을 구현하고  아래처럼 IApiService 인터페이스를 상속받아서 Packet 을 등록하고 Packet Handler 를 구현해주면 된다.
```c#

  public class SampleApi : IApiService
    {
        private ISystemPanel? _systemPanel;
        private ISender? _sender;
        private ILogger _log = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        public  async Task Init(ISystemPanel systemPanel, ISender sender)
        {
            this._systemPanel = systemPanel;
            this._sender = sender;
            await Task.CompletedTask;
        }

        public  IApiService Instance()
        {
            return new SampleApi();
        }

        // IHandlerRegister 는 Client 에서 전달되는 패킷을 처리할때 사용되고 
        // IBackendHandlerRegister 는 서버에서 전달되는 패킷을 처리할때 사용된다.
        public  void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(AuthenticateReq.Descriptor.Index, Authenticate);
            register.Add(HelloReq.Descriptor.Index, Hello);
            register.Add(Playhouse.Simple.Protocol.CloseSessionMsg.Descriptor.Index, CloseSessionMsg);
            register.Add(SendMsg.Descriptor.Index, SendMessage);
        }
     

        private async Task Authenticate(Packet packet, IApiSender apiSender)
        {
            var req = AuthenticateReq.Parser.ParseFrom(packet.Data);
            long accountId = req.UserId;

            apiSender.Authenticate(accountId);

            var message = new AuthenticateRes { UserInfo = accountId.ToString() };
            apiSender.Reply(new ReplyPacket (message));
            await Task.CompletedTask;
        }

        private async Task Hello(Packet packet, IApiSender apiSender)
        {
            var req = HelloReq.Parser.ParseFrom(packet.Data);
            apiSender.Reply(new ReplyPacket (new HelloRes { Message = "hello" }));
            await Task.CompletedTask;
        }

        private async Task SendMessage(Packet packet, IApiSender apiSender)
        {
            var recv = SendMsg.Parser.ParseFrom(packet.Data);
            apiSender.SendToClient(new Packet(new SendMsg() { Message = recv.Message}));
            await Task.CompletedTask;
        }

        private async Task CloseSessionMsg(Packet packet, IApiSender apiSender)
        {
            apiSender.SendToClient(new Packet (new CloseSessionMsg()));
            apiSender.SessionClose(apiSender.SessionEndpoint, apiSender.Sid);
            await Task.CompletedTask;
        }
    }
```

## Simple Play
Play 서버도 공통적으로 ServerSystem 인터페이스를 구현해주고 추가적으로 Stage 와  Actor 인터페이스를 구현해주면 된다.
제공되는 방생성 및 입장 callback을 구현하고  , OnDispatch 에서 Stage(room) 으로 전달되는 메시치 처리를 해주면 된다.
```c#
internal class SimpleRoom : IStage
{
    private ILogger _log = Log.Logger;
    private PacketHandler<SimpleRoom, SimpleUser> _handler = new ();
    private Dictionary<long,SimpleUser> _userMap = new ();
    private int _count;
    private IStageSender _stageSender;

    public IStageSender StageSender => _stageSender;

    public SimpleRoom(IStageSender stageSender)
    {
        _stageSender = stageSender;
        _handler.Add(LeaveRoomReq.Descriptor.Index, new LeaveRoomCmd());
        _handler.Add(ChatMsg.Descriptor.Index, new ChatMsgCmd());

        StageSender.AddCountTimer(TimeSpan.FromSeconds(3), 3, TimeSpan.FromSeconds(1), TimerCounter);
        StageSender.AddRepeatTimer(TimeSpan.FromSeconds(0),TimeSpan.FromMilliseconds(200), async () =>
        {
            _log.Information("repeat timer");
            await Task.CompletedTask;
        });
    }

    private async Task TimerCounter()
    {
        _log.Information($"count timer {_count++}");
        await Task.CompletedTask;

    }

    public async Task<ReplyPacket> OnCreate(Packet packet)
    {
        _log.Information($"OnCreate: stageType:{StageSender.StageType},stageId:{StageSender.StageId},${packet.MsgId}");
        var request = CreateRoomAsk.Parser.ParseFrom(packet.Data);

        await Task.CompletedTask;
        return new ReplyPacket(new CreateRoomAnswer() { Data = request.Data });
    }

    public async Task OnDisconnect(object actor)
    {
        SimpleUser user =(SimpleUser) actor;
        LeaveRoom(user);
        await Task.CompletedTask;
    }

    public void LeaveRoom(SimpleUser user)
    {
        _userMap.Remove(user.GetAccountId());
        _log.Information($"leave room {user.GetAccountId()}, size:{_userMap.Count}");

        if(_userMap.Count == 0)
        {
            _log.Information($"add count timer: ${Thread.CurrentThread.Name} ");
            StageSender.AddCountTimer(TimeSpan.FromSeconds(5), 1, TimeSpan.FromSeconds(5), async () => {
                if(_userMap.Count == 0)
                {
                    _log.Information($"close room : {Thread.CurrentThread.Name}");
                    StageSender.CloseStage();
                    await Task.CompletedTask;
                }
            });
        }
    }

    public async Task OnDispatch(object actor, Packet packet)
    {
        SimpleUser user = (SimpleUser)actor;
        await _handler.Dispatch(this, user, packet);
    }

    public async Task<ReplyPacket> OnJoinStage(object actor, Packet packet)
    {
        SimpleUser user = (SimpleUser)actor;
        var request = JoinRoomAsk.Parser.ParseFrom(packet.Data);

        await Task.CompletedTask;
        return new ReplyPacket(new JoinRoomAnswer() { Data = request.Data });

    }

    public async Task OnPostCreate()
    {
        await Task.CompletedTask;
    }

    public async Task OnPostJoinStage(object actor)
    {
        SimpleUser user = (SimpleUser)actor;

        _userMap[user.GetAccountId()] = user;
        List<Task<ReplyPacket>> requests = new List<Task<ReplyPacket>>();
        foreach (var item in _userMap.Values)
        {
            requests.Add(item.ActorSender.AsyncToApi(new Packet(new HelloReq() { Message = "Hello" })));
        }

        ReplyPacket[] replys = await Task.WhenAll(requests);

        
        foreach (var reply in replys)
        {
            var helloRes = HelloRes.Parser.ParseFrom(reply.Data);    
            _log.Information($"reply : {helloRes.Message}");
        }

    }

    internal void SendToAll(Packet packet)
    {
        foreach (var item in _userMap.Values)
        {
            item.ActorSender.SendToClient(packet);
        }
    }
}
```
Actor(User)
룸에서 생성되는 User객체이다. IActor 인터페이스를 상속받아서 구현하면 된다.
```c#
 internal class SimpleUser : IActor
    {
        private ILogger _log = Log.Logger;
        private IActorSender _sender;
        public IActorSender ActorSender => _sender;

        public SimpleUser(IActorSender sender) { 
            _sender = sender;
        }

        public async Task OnCreate()
        {
            _log.Information($"OnCreate {_sender.AccountId}");
            await Task.CompletedTask;
        }

        public async Task OnDestroy()
        {
            _log.Information($"OnDestroy {_sender.AccountId}");
            await Task.CompletedTask;
        }

        internal long GetAccountId() => ActorSender.AccountId();
    }
```
```C#
internal class PlayApplication
    {
        private ILogger _log = Log.Logger;
        internal void Run()
        {
            try
            {
                var redisPort = 6379;
                var commonOption = new CommonOption()
                {
                    Port = 30570,
                    ServiceId = 3,
                    RedisPort = redisPort,
                    ServerSystem = (systemPanel, sender) => new PlaySystem(systemPanel, sender),
                    RequestTimeoutSec = 0,
                };

                var playOption = new PlayOption();

                //playOption 에 위에서 정의한 SimpleRoom과 SimpleUser 를 생성자 함수로 등록해준다.
                //생성자 함수 Type 을 인자로 지정할수 있다. 룸 생성시 지정된 type으로 room 이 생성된다. 
                //즉 여러 type 의 Room 과 User 를 등록해서 사용 할수 있다.
                playOption.PlayProducer.Register(
                    "simple", 
                    (stageSender) => new SimpleRoom(stageSender),  
                    (actorSender) => new SimpleUser(actorSender)
                );

                var playServer = new PlayServer(commonOption, playOption);
                playServer.Start();

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    _log.Information("*** shutting down Play server since process is shutting down");
                    playServer.Stop();
                    _log.Information("*** server shut down");
                    Thread.Sleep(1000);
                };

                _log.Information("Play Server Started");
                playServer.AwaitTermination();

            }
            catch(Exception ex)
            {
                _log.Error(ex, ex.StackTrace ?? ex.Message);
            }
        }
    }
```

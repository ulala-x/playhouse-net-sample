using PlayHouse.Production.Play;
using PlayHouse.Production.Shared;
using PlayHouse.Utils;
using Simple;
using SimplePlay.Room.Command;
using SimpleProtocol;

namespace SimplePlay.Room;

internal class SimpleRoom : IStage
{
    private int _count;
    private readonly PacketHandler<SimpleRoom, SimpleUser> _handler = new();
    private readonly LOG<SimpleRoom> _log = new();
    private readonly Dictionary<long, SimpleUser> _userMap = new();

    public SimpleRoom(IStageSender stageSender)
    {
        StageSender = stageSender;
        _handler.Add(LeaveRoomReq.Descriptor.Name, new LeaveRoomCmd());
        _handler.Add(ChatMsg.Descriptor.Name, new ChatMsgCmd());

        StageSender.AddCountTimer(TimeSpan.FromSeconds(3), 3, TimeSpan.FromSeconds(1), TimerCounter);
        StageSender.AddRepeatTimer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(200), async () =>
        {
            _log.Debug(() => $"repeat timer");
            await Task.CompletedTask;
        });
    }

    public IStageSender StageSender { get; }

    public async Task<(ushort errorCode, IPacket reply)> OnCreate(IPacket packet)
    {
        _log.Debug(() =>
            $"OnCreate -  [stageType:{StageSender.StageType},stageId:{StageSender.StageId},msgId:${packet.MsgId}]");
        var request = packet.Parse<CreateRoomAsk>();

        await Task.CompletedTask;
        return (0, new SimplePacket(new CreateRoomAnswer { Data = request.Data }));
    }

    public async Task OnDisconnect(IActor actor)
    {
        var user = (SimpleUser)actor;
        _log.Debug(() =>
            $"OnDisconnect - [stageType:{StageSender.StageType},stageId:{StageSender.StageId},accountId:{user.AccountId}]");
        LeaveRoom(user);
        await Task.CompletedTask;
    }

    public async Task OnDispatch(IActor actor, IPacket packet)
    {
        var user = (SimpleUser)actor;
        _log.Debug(() =>
            $"onDispatch - [stageType:{StageSender.StageType},stageId:${StageSender.StageId},msgId:${packet.MsgId}]");
        await _handler.Dispatch(this, user, packet);
    }

    public async Task<(ushort errorCode, IPacket reply)> OnJoinStage(IActor actor, IPacket packet)
    {
        var user = (SimpleUser)actor;

        _log.Debug(() =>
            $"OnJoinStage - [stageType:{StageSender.StageType},stageId:${StageSender.StageId},msgId:${packet.MsgId}]");
        var request = packet.Parse<JoinRoomAsk>();

        await Task.CompletedTask;
        return (0, new SimplePacket(new JoinRoomAnswer { Data = request.Data }));
    }

    public async Task OnPostCreate()
    {
        _log.Debug(() => $"OnPostCreate - [stageType:{StageSender.StageType},stageId:${StageSender.StageId}]");
        await Task.CompletedTask;
    }

    public async Task OnPostJoinStage(IActor actor)
    {
        var user = (SimpleUser)actor;

        _userMap[user.AccountId] = user;
        _log.Debug(() =>
            $"OnPostJoinStage - [stageType:{StageSender.StageType},stageId:${StageSender.StageId},accountId:{user.AccountId}]");

        List<Task<IPacket>> requests = new();
        foreach (var item in _userMap.Values)
        {
            requests.Add(item.ActorSender.RequestToApi(new SimplePacket(new HelloToApiReq { Data = "Hello" })));
        }

        var replys = await Task.WhenAll(requests);


        foreach (var reply in replys)
        {
            var helloRes = reply.Parse<HelloToApiRes>();
            _log.Debug(() => $"hello res - [data:{helloRes.Data}]");
        }
    }

    private async Task TimerCounter()
    {
        _log.Debug(() => $"count timer - [count:{_count++}]");
        await Task.CompletedTask;
    }

    public void LeaveRoom(SimpleUser user)
    {
        _userMap.Remove(user.AccountId);
        _log.Debug(() => $"leave room {user.AccountId}, size:{_userMap.Count}");

        if (_userMap.Count == 0)
        {
            StageSender.CloseStage();
            //_log.Debug(() => $"add count timer - [threadName:${Thread.CurrentThread.Name}] ");
            //StageSender.AddCountTimer(TimeSpan.FromSeconds(5), 1, TimeSpan.FromSeconds(5), async () => {
            //    if(_userMap.Count == 0)
            //    {
            //        _log.Debug(() => $"close room - [threadName:{Thread.CurrentThread.Name}]");
            //        StageSender.CloseStage();
            //        await Task.CompletedTask;
            //    }
            //});
        }
    }


    internal void SendToAll(IPacket packet)
    {
        foreach (var item in _userMap.Values)
        {
            item.ActorSender.SendToClient(packet);
        }
    }
}
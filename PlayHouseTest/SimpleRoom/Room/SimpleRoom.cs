using PlayHouse.Production;
using PlayHouse.Production.Play;
using PlayHouse.Utils;
using Simple;
using SimplePlay.Room.Command;
namespace SimplePlay.Room
{
    internal class SimpleRoom : IStage
    {
        private LOG<SimpleRoom> _log = new();
        private PacketHandler<SimpleRoom, SimpleUser> _handler = new ();
        private Dictionary<string,SimpleUser> _userMap = new ();
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
                _log.Debug(()=>"repeat timer");
                await Task.CompletedTask;
            });
        }

        private async Task TimerCounter()
        {
            _log.Debug(()=>$"count timer - [count:{_count++}]");
            await Task.CompletedTask;

        }

        public async Task<ReplyPacket> OnCreate(Packet packet)
        {
            _log.Debug(() => $"OnCreate -  [stageType:{StageSender.StageType},stageId:{StageSender.StageId},msgId:${packet.MsgId}]");
            var request = CreateRoomAsk.Parser.ParseFrom(packet.Data);

            await Task.CompletedTask;
            return new ReplyPacket(new CreateRoomAnswer() { Data = request.Data });
        }

        public async Task OnDisconnect(object actor)
        {
            SimpleUser user =(SimpleUser) actor;
            _log.Debug(() => $"OnDisconnect - [stageType:{StageSender.StageType},stageId:{StageSender.StageId},accountId:{user.GetAccountId()}]");
            LeaveRoom(user);
            await Task.CompletedTask;
        }

        public void LeaveRoom(SimpleUser user)
        {
            _userMap.Remove(user.GetAccountId());
            _log.Debug(() => $"leave room {user.GetAccountId()}, size:{_userMap.Count}");

            if(_userMap.Count == 0)
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

        public async Task OnDispatch(object actor, Packet packet)
        {
            SimpleUser user = (SimpleUser)actor;
            _log.Debug(() => $"onDispatch - [stageType:{StageSender.StageType},stageId:${StageSender.StageId},msgId:${packet.MsgId}]");
            await _handler.Dispatch(this, user, packet);
        }

        public async Task<ReplyPacket> OnJoinStage(object actor, Packet packet)
        {
            SimpleUser user = (SimpleUser)actor;

            _log.Debug(() => $"OnJoinStage - [stageType:{StageSender.StageType},stageId:${StageSender.StageId},msgId:${packet.MsgId}]");
            var request = JoinRoomAsk.Parser.ParseFrom(packet.Data);

            await Task.CompletedTask;
            return new ReplyPacket(new JoinRoomAnswer() { Data = request.Data });
        }

        public async Task OnPostCreate()
        {
            _log.Debug(() => $"OnPostCreate - [stageType:{StageSender.StageType},stageId:${StageSender.StageId}]");
            await Task.CompletedTask;
        }

        public async Task OnPostJoinStage(object actor)
        {
            SimpleUser user = (SimpleUser)actor;

            _userMap[user.GetAccountId()] = user;
            _log.Debug(() => $"OnPostJoinStage - [stageType:{StageSender.StageType},stageId:${StageSender.StageId},accountId:{user.GetAccountId()}]");

            List<Task<ReplyPacket>> requests = new List<Task<ReplyPacket>>();
            foreach (var item in _userMap.Values)
            {
                requests.Add(item.ActorSender.AsyncToApi(new Packet(new HelloToApiReq() { Data = "Hello" })));
            }

            ReplyPacket[] replys = await Task.WhenAll(requests);

            
            foreach (var reply in replys)
            {
                var helloRes = HelloToApiReq.Parser.ParseFrom(reply.Data);    
                _log.Debug(() => $"hello res - [data:{helloRes.Data}]");
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
}

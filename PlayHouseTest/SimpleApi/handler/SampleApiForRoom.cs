using PlayHouse.Production.Api;
using PlayHouse.Production;
using Microsoft.Extensions.DependencyInjection;
using Simple;
using PlayHouse.Utils;
using SimpleProtocol;

namespace SimpleApi.handler
{
    public class SampleApiForRoom : IApiController
    {

        private LOG<SampleApiForRoom> _log = new();
        private readonly ISystemPanel _systemPanel;
        private Random _random = new Random();
        private static long _stageId = 1;

        public SampleApiForRoom()
        {
            _systemPanel = GlobalControlProvider.SystemPanel;
        }

        private const string RoomSvcId = "room";
        private const string RoomType = "simple";
        private const int Success = 0;
        private const int Fail = 1;
        private const ushort RoomServiceId = 2;

        public void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(CreateRoomReq.Descriptor.Index, CreateStage);
            register.Add(JoinRoomReq.Descriptor.Index, JoinStage);
            register.Add(CreateJoinRoomReq.Descriptor.Index, CreateJoinStage);

            backendRegister.Add(LeaveRoomNotify.Descriptor.Index, LeaveRoomNoti);
            backendRegister.Add(HelloToApiReq.Descriptor.Index, HelloToApi);
        }


        private async Task CreateStage(IPacket packet, IApiSender apiSender)
        {

            _log.Debug(()=>
                $"CreateRoom - accountId:{apiSender.AccountId}, msgName:{SimpleReflection.Descriptor.MessageTypes.First(mt => mt.Index == packet.MsgId).Name}"
            );

            var data = packet.Parse<CreateRoomReq>().Data;
            var randRoomServerInfo = _systemPanel!.GetServerInfoByService(RoomServiceId);

            var roomEndpoint = randRoomServerInfo.BindEndpoint();
            var stageId = Interlocked.Increment(ref _stageId).ToString();

            var result = await apiSender.CreateStage(roomEndpoint, RoomType, stageId, new SimplePacket(new CreateRoomAsk() { Data = data}));

            var createRoomAnswer = CreateRoomAnswer.Parser.ParseFrom(result.CreateStageRes.Payload.Data);

            _log.Debug(() => $"stageId:{stageId}");

            if (result.IsSuccess())
            {
                apiSender.Reply(new SimplePacket(new CreateRoomRes() {
                        Data = createRoomAnswer.Data,
                        StageId = stageId,
                        PlayEndpoint = roomEndpoint,
                }));
            }
            else
            {
                apiSender.Reply(result.ErrorCode);
            }
        }

        private async Task JoinStage(IPacket packet, IApiSender apiSender)
        {
            _log.Debug(() => $"joinRoom - accountId:{apiSender.AccountId}, sid:{apiSender.Sid}, msgName:{SimpleReflection.Descriptor.MessageTypes.First(x => x.Index == packet.MsgId).Name}");

            var request = packet.Parse<JoinRoomReq>();
            string data = request.Data;
            string stageId = request.StageId;
            string roomEndpoint = request.PlayEndpoint;


            JoinStageResult result = await apiSender.JoinStage(roomEndpoint, stageId, new SimplePacket(new JoinRoomAsk() { Data = data }));

            if (result.IsSuccess())
            {
                var joinRoomAnswer = result.JoinStageRes.Parse<JoinRoomAnswer>();
                apiSender.Reply(new SimplePacket(new JoinRoomRes{
                        Data = joinRoomAnswer.Data,
                        StageIdx = result.StageIndex,
                }));
            }
            else
            {
                apiSender.Reply(result.ErrorCode);
            }
        }

        private async Task CreateJoinStage(IPacket packet, IApiSender apiSender)
        {
            _log.Debug(() => $"CreateJoinRoomReq - accountId:{apiSender.AccountId},sid:{apiSender.Sid},msgName:{SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name}");

            var request = packet.Parse<CreateJoinRoomReq>();
            var data = request.Data;
            string stageId = request.StageId;
            var roomEndpoint = request.PlayEndpoint;
            var createPayload = new SimplePacket(new CreateRoomAsk() { Data = data,});
            
            var joinPayload = new SimplePacket(new JoinRoomAsk() { Data = data, });

            var result = await apiSender.CreateJoinStage(roomEndpoint, RoomType, stageId, createPayload, joinPayload);
            if (result.IsSuccess())
            {
                //var joinRoomAnswer = CreateJoinRoomAnswer.Parser.ParseFrom(result.JoinStageRes.Payload.Data);
                var joinRoomAnswer = result.JoinStageRes.Parse<JoinRoomAnswer>();
                apiSender.Reply(new SimplePacket(new CreateJoinRoomRes() { Data = joinRoomAnswer.Data, StageIdx = result.StageIndex }));
            }
            else
            {
                apiSender.Reply(result.ErrorCode);
            }
        }

        private async Task LeaveRoomNoti(IPacket packet, IApiBackendSender backendSender)
        {
            _log.Debug(() => $"LeaveRoomNoti : accountId:{backendSender.AccountId},msgName:{SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name}");


            var notify = packet.Parse<LeaveRoomNotify>();
            backendSender.SendToClient(notify.SessionEndpoint, notify.Sid, new SimplePacket(notify));
            await Task.CompletedTask;
        }

        private async Task HelloToApi(IPacket packet, IApiBackendSender backendSender)
        {
            _log.Debug(()=>$"HelloToApi : accountId:{backendSender.AccountId},msgName:{SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name}");

            string data = packet.Parse<HelloToApiReq>().Data;
            backendSender.Reply(new SimplePacket(new HelloToApiRes { Data = data }));
            await Task.CompletedTask;
        }

        public IApiController Instance()
        {
            return GlobalServiceProvider.Instance.GetService<SampleApiForRoom>()!;
        }
    }
}
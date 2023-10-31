using PlayHouse.Production.Api;
using PlayHouse.Production;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Simple;

namespace SimpleApi.handler
{
    public class SampleApiForRoom : IApiController
    {
     
        private readonly ILogger _log = Log.Logger;
        private readonly ISystemPanel _systemPanel;

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


        private async Task CreateStage(Packet packet, IApiSender apiSender)
        {

            _log.Debug(
                "CreateRoom : accountId:{0}, msgName:{1}",
                apiSender.AccountId,SimpleReflection.Descriptor.MessageTypes.First(mt => mt.Index == packet.MsgId).Name
            );

            var data = CreateRoomReq.Parser.ParseFrom(packet.Data).Data;
            var randRoomServerInfo = _systemPanel!.GetServerInfoByService(RoomServiceId);

            var roomEndpoint = randRoomServerInfo.BindEndpoint();
            var stageId = Guid.NewGuid();

            var result = await apiSender.CreateStage(roomEndpoint, RoomType, stageId, new Packet(new CreateRoomAsk() { Data = data}));

            var createRoomAnswer = CreateRoomAnswer.Parser.ParseFrom(result.CreateStageRes.Data);

            _log.Debug("stageId:{0}",stageId);

            if (result.IsSuccess())
            {
                apiSender.Reply(new ReplyPacket(new CreateRoomRes() {
                        Data = createRoomAnswer.Data,
                        StageId = ByteString.CopyFrom(stageId.ToByteArray()),
                        PlayEndpoint = roomEndpoint,
                }));
            }
            else
            {
                apiSender.Reply(new ReplyPacket(result.ErrorCode));
            }
        }

        private async Task JoinStage(Packet packet, IApiSender apiSender)
        {
            _log.Debug("joinRoom : accountId:{0}, sid:{1}, msgName:{2}", 
                apiSender.AccountId,apiSender.Sid,SimpleReflection.Descriptor.MessageTypes.First(x => x.Index == packet.MsgId).Name
            );

            var request = JoinRoomReq.Parser.ParseFrom(packet.Data);
            string data = request.Data;
            Guid stageId = new Guid(request.StageId.ToByteArray());
            string roomEndpoint = request.PlayEndpoint;


            var result = await apiSender.JoinStage(roomEndpoint, stageId, new Packet(new JoinRoomAsk() { Data = data }));

            if (result.IsSuccess())
            {
                _log.Debug("StageIdx:{0}",result.StageIndex);

                var joinRoomAnswer = JoinRoomAnswer.Parser.ParseFrom(result.JoinStageRes.Data);
                apiSender.Reply(new ReplyPacket(new JoinRoomRes{
                        Data = joinRoomAnswer.Data,
                        StageIdx = result.StageIndex,
                }));
            }
            else
            {
                apiSender.Reply(new ReplyPacket(result.ErrorCode));
            }
        }

        private async Task CreateJoinStage(Packet packet, IApiSender apiSender)
        {
            _log.Debug("CreateJoinRoomReq : accountId:{0},sid:{1},msgName:{2}"
                ,apiSender.AccountId,apiSender.Sid,SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name);

            var request = CreateJoinRoomReq.Parser.ParseFrom(packet.Data);
            var data = request.Data;
            Guid stageId = new Guid(request.StageId.ToByteArray());
            var roomEndpoint = request.PlayEndpoint;
            var createPayload = new Packet(new CreateRoomAsk() { Data = data,});
            
            var joinPayload = new Packet(new JoinRoomAsk() { Data = data, });

            var result = await apiSender.CreateJoinStage(roomEndpoint, RoomType, stageId, createPayload, joinPayload);
            if (result.IsSuccess())
            {
                var joinRoomAnswer = CreateJoinRoomAnswer.Parser.ParseFrom(result.JoinStageRes.Data);
                apiSender.Reply(new ReplyPacket(new CreateJoinRoomRes() { Data = joinRoomAnswer.Data, StageIdx = result.StageIndex }));
            }
            else
            {
                apiSender.Reply(new ReplyPacket(result.ErrorCode));
            }
        }

        private async Task LeaveRoomNoti(Packet packet, IApiBackendSender backendSender)
        {
            _log.Debug("LeaveRoomNoti : accountId:{0},,msgName:{1}"
                ,backendSender.AccountId,SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name);


            var notify = LeaveRoomNotify.Parser.ParseFrom(packet.Data);
            backendSender.SendToClient(notify.SessionEndpoint, notify.Sid, new Packet(notify));
            await Task.CompletedTask;
        }

        private async Task HelloToApi(Packet packet, IApiBackendSender backendSender)
        {
            _log.Debug("HelloToApi : accountId:{0},,msgName:{1}"
                ,backendSender.AccountId,SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name);

            string data = HelloToApiReq.Parser.ParseFrom(packet.Data).Data;
            backendSender.Reply(new ReplyPacket(new HelloToApiRes { Data = data }));
            await Task.CompletedTask;
        }

        public IApiController Instance()
        {
            return GlobalServiceProvider.Instance.GetService<SampleApiForRoom>()!;
        }
    }
}
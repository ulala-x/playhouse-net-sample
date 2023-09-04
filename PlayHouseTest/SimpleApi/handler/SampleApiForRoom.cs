using PlayHouse.Production.Api;
using PlayHouse.Production;
using Playhouse.Simple.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PlayHouse.Service.Api;
using PlayHouse.Service;
using Google.Protobuf.Reflection;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleApi.handler
{
    public class SampleApiForRoom : IApiController
    {
     
        private readonly ILogger<SampleApiForRoom> _log;
        private ISystemPanel _systemPanel;

        public SampleApiForRoom(ILogger<SampleApiForRoom> log)
        {
            _systemPanel = GlobalControlProvider.SystemPanel;
            _log = log;
        }


        private const string _roomSvcId = "room";
        private const string _roomType = "simple";
        private const int _success = 0;
        private const int _fail = 1;
        private const short _roomServiceId = 3;

        public void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(CreateRoomReq.Descriptor.Index, CreateStage);
            register.Add(JoinRoomReq.Descriptor.Index, JoinStage);
            register.Add(CreateJoinRoomReq.Descriptor.Index, CreateJoinStage);

            backendRegister.Add(LeaveRoomNotify.Descriptor.Index, LeaveRoomNoti);
            backendRegister.Add(HelloReq.Descriptor.Index, HelloToApi);
        }


        private async Task CreateStage(Packet packet, IApiSender apiSender)
        {

            _log.LogInformation($"CreateRoom : accountId:{apiSender.AccountId}, msgName:{SimpleReflection.Descriptor.MessageTypes.First(mt => mt.Index == packet.MsgId).Name}");

            var data = CreateRoomReq.Parser.ParseFrom(packet.Data).Data;
            var randRoomServerInfo = _systemPanel!.GetServerInfoByService(_roomServiceId);

            var roomEndpoint = randRoomServerInfo.BindEndpoint();
            var stageId = Guid.NewGuid();

            var result = await apiSender.CreateStage(roomEndpoint, _roomType, stageId, new Packet(new CreateRoomAsk() { Data = data}));

            var createRoomAnswer = CreateRoomAnswer.Parser.ParseFrom(result.CreateStageRes.Data);

            _log.LogInformation($"stageId:{stageId}");

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
        public async Task JoinStage(Packet packet, IApiSender apiSender)
        {
            _log.LogInformation($"joinRoom : accountId:{apiSender.AccountId}, sid:{apiSender.Sid}, msgName:{SimpleReflection.Descriptor.MessageTypes.First(x => x.Index == packet.MsgId).Name}");

            var request = JoinRoomReq.Parser.ParseFrom(packet.Data);
            string data = request.Data;
            Guid stageId = new Guid(request.StageId.ToByteArray());
            string roomEndpoint = request.PlayEndpoint;

            //new Packet { Data = JoinRoomAsk.Parser.ParseFrom(ByteString.CopyFromUtf8(data)).ToByteString() }

            var result = await apiSender.JoinStage(roomEndpoint, stageId, new Packet(new JoinRoomAsk() { Data = data }));

            if (result.IsSuccess())
            {
                _log.LogInformation($"stageIdx:{result.StageIndex}");

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

        public async Task CreateJoinStage(Packet packet, IApiSender apiSender)
        {
            _log.LogInformation($"CreateJoinRoomReq : accountId:{apiSender.AccountId},sid:{apiSender.Sid}" +
                     $"msgName:{SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name}");

            var request = CreateJoinRoomReq.Parser.ParseFrom(packet.Data);
            var data = request.Data;
            Guid stageId = new Guid(request.StageId.ToByteArray());
            var roomEndpoint = request.PlayEndpoint;
            var createPayload = new Packet(new CreateRoomAsk() { Data = data,});
            
            var joinPayload = new Packet(new JoinRoomAsk() { Data = data, });

            var result = await apiSender.CreateJoinStage(roomEndpoint, _roomType, stageId, createPayload, joinPayload);
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
            _log.LogInformation($"leaveRoomNotify : accountId:{backendSender.AccountId}" +
                     $"msgName:{SimpleReflection.Descriptor.MessageTypes.First(x => x.Index == packet.MsgId).Name}");

            var notify = LeaveRoomNotify.Parser.ParseFrom(packet.Data);
            backendSender.SendToClient(notify.SessionEndpoint, notify.Sid, new Packet(notify));
            await Task.CompletedTask;
        }

        public async Task HelloToApi(Packet packet, IApiBackendSender backendSender)
        {
            _log.LogInformation($"helloToApiReq : accountId:{backendSender.AccountId}" +
                      $"msgName:{SimpleReflection.Descriptor.MessageTypes.First(m => m.Index == packet.MsgId)?.Name}");

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
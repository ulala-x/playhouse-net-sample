using PlayHouse.Production.Api;
using Simple;
using PlayHouse.Utils;
using SimpleProtocol;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Shared;

namespace SimpleApi.handler;

public class SampleApiForRoom : IApiController
{
    private LOG<SampleApiForRoom> _log = new();
    private readonly ISystemPanel _systemPanel;

    public SampleApiForRoom()
    {
        _systemPanel = ControlContext.SystemPanel;
    }

    private const string RoomType = "simple";
    private const ushort RoomServiceId = 2;

    public void Handles(IHandlerRegister register)
    {
        register.Add(CreateRoomReq.Descriptor.Index, CreateStage);
        register.Add(JoinRoomReq.Descriptor.Index, JoinStage);
        register.Add(CreateJoinRoomReq.Descriptor.Index, CreateJoinStage);
    }

    private async Task CreateStage(IPacket packet, IApiSender apiSender)
    {

        _log.Debug(()=>
            $"CreateRoom - accountId:{apiSender.AccountId}, msgName:{SimpleReflection.Descriptor.MessageTypes.First(mt => mt.Index == packet.MsgId).Name}"
        );

        var data = packet.Parse<CreateRoomReq>().Data;
        var randRoomServerInfo = _systemPanel!.GetServerInfoBy(RoomServiceId);

        var roomEndpoint = randRoomServerInfo.GetBindEndpoint();
        long stageId = _systemPanel.GenerateUUID();

        CreateStageResult result = await apiSender.CreateStage(roomEndpoint, RoomType, stageId, new SimplePacket(new CreateRoomAsk() { Data = data}));

        var createRoomAnswer = CreateRoomAnswer.Parser.ParseFrom(result.CreateStageRes.Payload.Data.Span);

        _log.Debug(() => $"stageId:{stageId}");

        if (result.IsSuccess())
        {
            apiSender.Reply(new SimplePacket(new CreateRoomRes() {
                    Data = createRoomAnswer.Data,
                    PlayEndpoint = roomEndpoint,
                    StageId = stageId,
            }));
        }
        else
        {
            apiSender.Reply(result.ErrorCode);
        }
    }

    private async Task JoinStage(IPacket packet, IApiSender apiSender)
    {
        

        var request = packet.Parse<JoinRoomReq>();
        string data = request.Data;
        long stageId = request.StageId;
        string roomEndpoint = request.PlayEndpoint;

        _log.Debug(() => $"joinRoom - accountId:{apiSender.AccountId},sid:{apiSender.Sid},stageId:{stageId} msgName:{SimpleReflection.Descriptor.MessageTypes.First(x => x.Index == packet.MsgId).Name}");


        JoinStageResult result = await apiSender.JoinStage(roomEndpoint, stageId, new SimplePacket(new JoinRoomAsk() { Data = data }));

        if (result.IsSuccess())
        {
            var joinRoomAnswer = result.JoinStageRes.Parse<JoinRoomAnswer>();
            apiSender.Reply(new SimplePacket(new JoinRoomRes{
                    Data = joinRoomAnswer.Data
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
        long stageId = _systemPanel.GenerateUUID(); 
        var roomEndpoint = request.PlayEndpoint;
        var createPayload = new SimplePacket(new CreateRoomAsk() { Data = data,});
        
        var joinPayload = new SimplePacket(new JoinRoomAsk() { Data = data, });

        var result = await apiSender.CreateJoinStage(roomEndpoint, RoomType, stageId, createPayload, joinPayload);
        if (result.IsSuccess())
        {
            //var joinRoomAnswer = CreateJoinRoomAnswer.Parser.ParseFrom(result.JoinStageRes.Payload.Data);
            var joinRoomAnswer = result.JoinStageRes.Parse<JoinRoomAnswer>();
            apiSender.Reply(new SimplePacket(new CreateJoinRoomRes() { Data = joinRoomAnswer.Data, StageId= stageId }));
        }
        else
        {
            apiSender.Reply(result.ErrorCode);
        }
    }
}

public class SampleBackendApiForRoom : IApiBackendController
{
    private LOG<SampleBackendApiForRoom> _log = new();

    public SampleBackendApiForRoom()
    {
    }

    public void Handles(IBackendHandlerRegister backendRegister)
    {
        backendRegister.Add(LeaveRoomNotify.Descriptor.Index, LeaveRoomNoti);
        backendRegister.Add(HelloToApiReq.Descriptor.Index, HelloToApi);
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
        _log.Debug(() => $"HelloToApi : accountId:{backendSender.AccountId},msgName:{SimpleReflection.Descriptor.MessageTypes.Single(m => m.Index == packet.MsgId).Name}");

        string data = packet.Parse<HelloToApiReq>().Data;
        backendSender.Reply(new SimplePacket(new HelloToApiRes { Data = data }));
        await Task.CompletedTask;
    }

}
using PlayHouse.Production.Api;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Shared;
using PlayHouse.Utils;
using Simple;
using SimpleProtocol;

namespace SimpleApi.handler;

public class SampleApiForRoom : IApiController
{
    private const string RoomType = "simple";
    private const ushort RoomServiceId = 2;
    private readonly ISystemPanel _systemPanel;
    private readonly LOG<SampleApiForRoom> _log = new();

    public SampleApiForRoom()
    {
        _systemPanel = ControlContext.SystemPanel;
    }

    public void Handles(IHandlerRegister register)
    {
        register.Add(CreateRoomReq.Descriptor.Name, CreateStage);
        register.Add(JoinRoomReq.Descriptor.Name, JoinStage);
        register.Add(CreateJoinRoomReq.Descriptor.Name, CreateJoinStage);
    }

    private async Task CreateStage(IPacket packet, IApiSender apiSender)
    {
        _log.Debug(() =>
            $"CreateRoom - [accountId:{apiSender.AccountId}, msgName:{packet.MsgId}]"
        );

        var data = packet.Parse<CreateRoomReq>().Data;
        var randRoomServerInfo = _systemPanel!.GetServerInfoBy(RoomServiceId);

        var roomEndpoint = randRoomServerInfo.GetBindEndpoint();
        var stageId = _systemPanel.GenerateUUID();

        var result = await apiSender.CreateStage(roomEndpoint, RoomType, stageId,
            new SimplePacket(new CreateRoomAsk { Data = data }));

        var createRoomAnswer = CreateRoomAnswer.Parser.ParseFrom(result.CreateStageRes.Payload.Data.Span);

        _log.Debug(() => $"stageId:{stageId}");

        if (result.IsSuccess())
        {
            apiSender.Reply(new SimplePacket(new CreateRoomRes
            {
                Data = createRoomAnswer.Data,
                PlayEndpoint = roomEndpoint,
                StageId = stageId
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
        var data = request.Data;
        var stageId = request.StageId;
        var roomEndpoint = request.PlayEndpoint;

        _log.Debug(() =>
            $"joinRoom - [accountId:{apiSender.AccountId},sid:{apiSender.Sid},stageId:{stageId} msgName:{packet.MsgId}]");


        var result =
            await apiSender.JoinStage(roomEndpoint, stageId, new SimplePacket(new JoinRoomAsk { Data = data }));

        if (result.IsSuccess())
        {
            var joinRoomAnswer = result.JoinStageRes.Parse<JoinRoomAnswer>();
            apiSender.Reply(new SimplePacket(new JoinRoomRes
            {
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
        _log.Debug(() =>
            $"CreateJoinRoomReq - [accountId:{apiSender.AccountId},sid:{apiSender.Sid},msgName:{packet.MsgId}]");

        var request = packet.Parse<CreateJoinRoomReq>();
        var data = request.Data;
        var stageId = _systemPanel.GenerateUUID();
        var roomEndpoint = request.PlayEndpoint;
        var createPayload = new SimplePacket(new CreateRoomAsk { Data = data });

        var joinPayload = new SimplePacket(new JoinRoomAsk { Data = data });

        var result = await apiSender.CreateJoinStage(roomEndpoint, RoomType, stageId, createPayload, joinPayload);
        if (result.IsSuccess())
        {
            //var joinRoomAnswer = CreateJoinRoomAnswer.Parser.ParseFrom(result.JoinStageRes.Payload.Data);
            var joinRoomAnswer = result.JoinStageRes.Parse<JoinRoomAnswer>();
            apiSender.Reply(new SimplePacket(new CreateJoinRoomRes { Data = joinRoomAnswer.Data, StageId = stageId }));
        }
        else
        {
            apiSender.Reply(result.ErrorCode);
        }
    }
}

public class SampleBackendApiForRoom : IApiBackendController
{
    private readonly LOG<SampleBackendApiForRoom> _log = new();

    public void Handles(IBackendHandlerRegister backendRegister)
    {
        backendRegister.Add(LeaveRoomNotify.Descriptor.Name, LeaveRoomNoti);
        backendRegister.Add(HelloToApiReq.Descriptor.Name, HelloToApi);
    }


    private async Task LeaveRoomNoti(IPacket packet, IApiBackendSender backendSender)
    {
        _log.Debug(() =>
            $"CreateRoom - [accountId: {backendSender.AccountId} ,msgName: {packet.MsgId}");


        var notify = packet.Parse<LeaveRoomNotify>();
        backendSender.SendToClient(notify.SessionEndpoint, notify.Sid, new SimplePacket(notify));
        await Task.CompletedTask;
    }

    private async Task HelloToApi(IPacket packet, IApiBackendSender backendSender)
    {
        _log.Debug(() =>
            $"CreateRoom - [accountId: {backendSender.AccountId} ,msgName: {packet.MsgId}");

        var data = packet.Parse<HelloToApiReq>().Data;
        backendSender.Reply(new SimplePacket(new HelloToApiRes { Data = data }));
        await Task.CompletedTask;
    }
}
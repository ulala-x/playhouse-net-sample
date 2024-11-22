using PlayHouse.Utils;
using PlayHouseConnector;
using Simple;
using Packet = PlayHouseConnector.Packet;

namespace SimpleClient;

public class RandomStringGenerator
{
    private static readonly Random random = new();

    public static string GenerateRandomString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        for (var i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}

internal class ClientApplication
{
    private readonly ushort _apiSvcId = 1;
    private readonly LOG<ClientApplication> _log = new();
    private readonly ushort _playSvcId = 2;
    private readonly AtomicShort _sequence = new();


    public async Task RunAsync()
    {
        var debugMode = false;
        var connector = new Connector();
        connector.Init(new ConnectorConfig
        {
            RequestTimeoutMs = 6000, EnableLoggingResponseTime = true, Host = "127.0.0.1", Port = 10114,
            HeartBeatIntervalMs = 1000, ConnectionIdleTimeoutMs = 30000
        });

        connector.OnReceive += (serviceId, packet) =>
        {
            // _log.Information($"message onReceive - [serviceId:{serviceId},msgId:{packet.MsgId}]");

            if (serviceId == _apiSvcId)
            {
                if (packet.MsgId == SendMsg.Descriptor.Name)
                {
                    _log.Debug(() =>
                        $"api SendMsg message - [msgId:{packet.MsgId}, message:{SendMsg.Parser.ParseFrom(packet.DataSpan).Message}]");
                }
            }
            else if (serviceId == _playSvcId)
            {
                if (packet.MsgId == ChatMsg.Descriptor.Name)
                {
                    _log.Debug(() =>
                        $"stage chat msg -  [msgId:{packet.MsgId},data:{ChatMsg.Parser.ParseFrom(packet.DataSpan).Data}]");
                }
            }
        };

        connector.OnDisconnect += () => { _log.Info(() => $"onDisconnect"); };

        //connector.OnError += (serviceId,errorCode,request) =>
        //{

        //};

        var thread = new Thread(() =>
        {
            while (true)
            {
                connector.MainThreadAction();
                Thread.Sleep(10);
            }
        });
        thread.Start();
        //_timer = new Timer((arg) => { connector.MainThreadAction();}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));

        connector.ClearCache();
        var result = await connector.ConnectAsync(debugMode);
        long accountId = _sequence.IncrementAndGet();
        _log.Info(() => $"onConnect - [accountId:{accountId},result:{result}]");

        if (result == false)
        {
            return;
        }

        try
        {
            string msg = "1234567890qwertyuiop";
            var statusCheckRes = await connector.RequestAsync((ushort)ServiceId.Session, new Packet(new AccessQueueStatusCheckReq
            {
                Data = msg
            }));
            var accessQueueStatusCheckRes = Simple.AccessQueueStatusCheckRes.Parser.ParseFrom(statusCheckRes.DataSpan);

            _log.Debug(() =>
                $"AccessQueueStatusCheckRes - [AccessQueueStatusCheckRes:{accessQueueStatusCheckRes.Data}]");

            var response = await connector.AuthenticateAsync(_apiSvcId,
                new Packet(new AuthenticateReq { PlatformUid = accountId.ToString(), Token = "password" }));

            var authenticateRes = AuthenticateRes.Parser.ParseFrom(response.DataSpan);
            _log.Debug(() =>
                $"AuthenticateRes - [accountId:{authenticateRes.AccountId},userId:{authenticateRes.UserInfo}]");

            var offSet = 2;

            for (var i = offSet; i < offSet + 10; i++)
            {
                if (false == connector.IsConnect())
                {
                    return;
                }

                var helloRes = await connector.RequestAsync(_apiSvcId, new Packet(new HelloReq { Message = "hi!" }));

                _log.Debug(() =>
                    $"response message - [accountId:{accountId},count:{i},message:{HelloRes.Parser.ParseFrom(helloRes.DataSpan).Message}]");
            }


            connector.Send(_apiSvcId, new Packet(new CloseSessionMsg()));

            await Task.Delay(1000);

            connector.ClearCache();
            await connector.ConnectAsync(debugMode);
            _log.Info(() => $"Reconnect");
            Thread.Sleep(TimeSpan.FromSeconds(2));

            _log.Info(() => $"before AuthenticateReq - [accountId:{accountId}]");
            response = await connector.AuthenticateAsync(_apiSvcId,
                new Packet(new AuthenticateReq { PlatformUid = accountId.ToString(), Token = "password" }));

            authenticateRes = AuthenticateRes.Parser.ParseFrom(response.DataSpan);
            _log.Info(() =>
                $"AuthenticateRes - [accountId:{authenticateRes.AccountId},userId:{authenticateRes.UserInfo}]");

            var compressResPacket = await connector.RequestAsync(_apiSvcId, 
                new Packet(new CompressReq()));

            var compressRes = CompressRes.Parser.ParseFrom(compressResPacket.DataSpan);
            _log.Info(()=>$"compress res - {compressRes.Data}");

            var duplicatePacketRes = await connector.RequestAsync(_apiSvcId,  
                new Packet(new DuplicatedPacketReq{Data = 1}), 10000);

            var duplicateRes = DuplicatedPacketRes.Parser.ParseFrom(duplicatePacketRes.DataSpan);

            _log.Info(() => $"duplicate 1 - res - {duplicateRes.Data}");

            var duplicatePacketRes2 = await connector.RequestAsync(_apiSvcId, 
                new Packet(new DuplicatedPacketReq { Data = 2 }),10000);

            var duplicateRes2 = DuplicatedPacketRes.Parser.ParseFrom(duplicatePacketRes2.DataSpan);

            _log.Info(() => $"duplicate 2 - res - {duplicateRes2.Data}");

            for (var i = 0; i < 10; ++i)
            {
                response = await connector.RequestAsync(_apiSvcId,
                    new Packet(new CreateRoomReq { Data = "success 1" }));

                var createRoomRes = CreateRoomRes.Parser.ParseFrom(response.DataSpan);
                var stageId = createRoomRes.StageId;
                var playEndpoint = createRoomRes.PlayEndpoint;

                _log.Debug(() =>
                    $"createroom - [playendpoint:{playEndpoint},stageId:{stageId},data:{createRoomRes.Data}]");

                response = await connector.RequestAsync(_apiSvcId,
                    new Packet(new JoinRoomReq { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 2" }));


                var joinRoomRes = JoinRoomRes.Parser.ParseFrom(response.DataSpan);
                _log.Debug(() =>
                    $"JoinRoomRes - [stageId:{stageId},data:{joinRoomRes.Data}]");

                response = await connector.RequestAsync(_playSvcId, stageId,
                    new Packet(new LeaveRoomReq { Data = "success 3" }));
                var leaveRoomRes = LeaveRoomRes.Parser.ParseFrom(response.DataSpan);

                _log.Debug(() =>
                    $"LeaveRoomRes - [data:{leaveRoomRes.Data}]");

                //}
                //var playEndpoint = "tcp://10.12.20.59:10570";
                //var stageId = ByteString.CopyFrom(string.Newstring().ToByteArray());

                response = await connector.RequestAsync(_apiSvcId,
                    new Packet(new CreateJoinRoomReq { PlayEndpoint = playEndpoint, Data = "success 4" }));

                var createJoinRoomRes = CreateJoinRoomRes.Parser.ParseFrom(response.DataSpan);
                stageId = createJoinRoomRes.StageId;

                _log.Debug(() =>
                    $"JoinRoomRes - [stageId: {stageId},data:{createJoinRoomRes.Data}]");

                connector.Send(_playSvcId, stageId, new Packet(new ChatMsg { Data = "hi!" }));
                connector.Send(_apiSvcId, new Packet(new SendMsg { Message = "hello!!" }));

                response = await connector.RequestAsync(_playSvcId, stageId,
                    new Packet(new LeaveRoomReq { Data = "success 5" }));

                var createJoinRoomLeaveRes = LeaveRoomRes.Parser.ParseFrom(response.DataSpan);
                _log.Debug(() =>
                    $"createJoinRoomLeaveRes - [data:{createJoinRoomLeaveRes.Data}]");

                for (int k = 0; k < 12; k++)
                {
                   connector.Request(_apiSvcId, new Packet(new Action_PlayActionReq
                    {
                        Type = k + 100000,
                        Value1 = k + 3000000000,
                        Value2 = k + 4000000000,
                        Value3 = k + 5000000000
                    }), packet =>
                   {
                       var actionRes = Action_PlayActionRes.Parser.ParseFrom(packet.DataSpan);

                       _log.Debug(() =>
                           $"playActionReq - [res:{actionRes}]");
                   } );
                    
                }
            }

            try
            {
                await connector.RequestAsync(_apiSvcId, new Packet(new TestNotRegisterReq()));
            }
            catch (PlayConnectorException ex)
            {
                _log.Error(() => $"Request Error - [accountId:{accountId},{ex.Message}]");
            }

            try
            {
                await connector.RequestAsync(_apiSvcId, new Packet(new TestTimeoutReq()));
            }
            catch (PlayConnectorException ex)
            {
                _log.Error(() => $"Request Error - [accountId:{accountId},{ex.Message}]");
            }
        }
        catch (PlayConnectorException ex)
        {
            _log.Error(() => $"Request Error - [accountId:{accountId},ex:{ex.Message}]");
        }
        catch (Exception ex)
        {
            _log.Error(() => $"Exception- [accountId:{accountId},ex:{ex}]");
        }

        await Task.Delay(TimeSpan.FromSeconds(6));
        //Environment.Exit(0);

        _log.Info(() =>$"finish");
        //await _timer.DisposeAsync();
        Environment.Exit(0);
    }
}
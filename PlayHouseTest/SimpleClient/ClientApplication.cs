using Playhouse.Simple.Protocol;
using PlayHouseConnector;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClient
{
    internal class ClientApplication
    {
        private ILogger _log = Log.Logger;

        public async Task RunAsync()
        {
            var connector = new Connector(new ConnectorConfig() {ReqestTimeout= 0 });

            connector.OnApiReceive += (serviceId, packet) =>
            {
                _log.Information($"OnApiReceive : serviceId:{serviceId},msgId:{packet.MsgId}");

                if(packet.MsgId == ChatMsg.Descriptor.Index)
                {
                    _log.Information($"api chat message : {ChatMsg.Parser.ParseFrom(packet.Data).Data}");
                }
            };

            connector.OnStageReceive += (serviceId, stageIndex, packet) =>
            {
                _log.Information($"stage message onReceive: serviceId:{serviceId},stageIndex:{stageIndex},msgId:{packet.MsgId}");

                if(packet.MsgId != ChatMsg.Descriptor.Index) 
                {
                    _log.Information($"stage chat msg: {ChatMsg.Parser.ParseFrom(packet.Data).Data}");
                }

            };

            connector.OnConnect += () =>
            {
                _log.Information("onConnect");
            };

            connector.OnDiconnect += () =>
            {
                _log.Information("onDisconnect");
            };

            ushort apiServicId = 1;
            ushort playServiceId = 2;

            connector.Start();
            connector.Connect("127.0.0.1", 30114);

            while (!connector.IsConnect())
                Thread.Yield();

            var response = await connector.RequestToApi(apiServicId, new Packet(new AuthenticateReq() { PlatformUid = "10", Token = "passowrd" }));
            
            if (!response.IsSuccess())
            {
                _log.Error($"request is not success , error:{response.ErrorCode}");
                Environment.Exit(0);
                
            }
            var authenticateRes = AuthenticateRes.Parser.ParseFrom(response.Data);
            
            _log.Information($"auth userInfo:{authenticateRes.UserInfo}");

            response = await connector.RequestToApi(apiServicId, new Packet(new HelloReq() { Message = "hi!" }));

            _log.Information($"response message : {HelloRes.Parser.ParseFrom(response.Data).Message}");


            connector.SendToApi(apiServicId, new Packet(new CloseSessionMsg()));

            Thread.Sleep(1000);


            connector.Connect("127.0.0.1", 30114);

            _log.Information("start Recconect");

            while (!connector.IsConnect())
                Thread.Yield();

            _log.Information("Recconected");

            response = await connector.RequestToApi(apiServicId, new Packet(new AuthenticateReq() { PlatformUid = "10", Token = "passowrd" }));

            if (!response.IsSuccess())
            {
                _log.Error($"request is not success , error:{response.ErrorCode}");
                Environment.Exit(0);
            }

            response = await connector.RequestToApi(apiServicId, new Packet(new CreateRoomReq() { Data = "success 1" }));

            if (!response.IsSuccess())
            {
                _log.Error($"request is not success , error:{response.ErrorCode}");
                Environment.Exit(0);
            }

            var createRoomRes = CreateRoomRes.Parser.ParseFrom(response.Data);
            var stageId = createRoomRes.StageId;
            var playEndpoint = createRoomRes.PlayEndpoint;

            _log.Information($"CreateRoom: PlayEndpoint:{playEndpoint},StageId:{stageId}");

            response = await connector.RequestToApi(apiServicId, new Packet(new JoinRoomReq() { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 2" }));

            if (!response.IsSuccess())
            {
                _log.Error($"request is not success , error:{response.ErrorCode}");
                Environment.Exit(0);
            }

            var joinRoomRes = JoinRoomRes.Parser.ParseFrom(response.Data);
            var stageIndex = joinRoomRes.StageIdx;

            _log.Information($"JoinRoomRes: stageIndex:{stageIndex}");

            response = await connector.RequestToStage(playServiceId, stageIndex, new Packet(new LeaveRoomReq() { Data = "success 3" }));
            if (!response.IsSuccess())
            {
                _log.Error($"request is not success , error:{response.ErrorCode}");
                Environment.Exit(0);
            }

            response = await connector.RequestToApi(apiServicId, new Packet(new CreateJoinRoomReq() { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 4" }));
            if (!response.IsSuccess())
            {
                _log.Error($"request is not success , error:{response.ErrorCode}");
                Environment.Exit(0);
            }

            connector.SendToApi(apiServicId, new Packet(new SendMsg() { Message = "hi!" }));

            await Task.Delay(TimeSpan.FromSeconds(1));

            _log.Information("finish");
            Environment.Exit(0);

        }
    }
}

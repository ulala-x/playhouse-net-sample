using PlayHouseConnector;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace SimpleClient
{
    internal class ClientApplication
    {
        private ILogger _log = Log.Logger;
        private ushort _apiServicId = 1;
        private ushort _playServiceId = 2;
        Stopwatch _stopwatch = new Stopwatch();

        public async Task RunAsync()
        {
            var connector = new Connector(new ConnectorConfig() {ReqestTimeout= 5 });

            connector.OnReceive += (serviceId, stageIndex, packet) =>
            {
                _log.Information($"stage message onReceive: [serviceId:{serviceId},stageIndex:{stageIndex},msgId:{packet.MsgId}]");

                if (serviceId == _apiServicId)
                {
                    if(packet.MsgId == SendMsg.Descriptor.Index)
                    {
                        _log.Information($"api SendMsg message : [msgId:{packet.MsgId}, message:{ChatMsg.Parser.ParseFrom(packet.Data).Data}]");
                    }    
                }
                else if (serviceId == _playServiceId)
                {
                    if(packet.MsgId == ChatMsg.Descriptor.Index) 
                    {
                        _log.Information($"stage chat msg: [msgId:{packet.MsgId},data:{ChatMsg.Parser.ParseFrom(packet.Data).Data}]");
                    }
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

            connector.Start();
            await connector.ConnectAsync("127.0.0.1", 10114);

            var response = await connector.RequestToApi(_apiServicId, new Packet(new AuthenticateReq() { PlatformUid = Guid.NewGuid().ToString(), Token = "passowrd" }));
            
            if (!response.IsSuccess())
            {
                _log.Error($"Authenticate is not success , error:{response.ErrorCode}");
                //Environment.Exit(0);
                
            }
            var authenticateRes = AuthenticateRes.Parser.ParseFrom(response.Data);
            
            //_log.Debug($"authenticate ok,auth userInfo:{authenticateRes.UserInfo}");
            
            for (int i = 0; i < 100; i++)
            {
                if (false == connector.IsConnect())
                {
                    return;
                }
                
                _stopwatch.Reset();
                _stopwatch.Start();
            
                response = await connector.RequestToApi(_apiServicId, new Packet(new HelloReq() { Message = "hi!" }));

                if (!response.IsSuccess())
                {
                    _log.Error($"request:{HelloReq.Descriptor.Name},errorCode:{response.ErrorCode}");
                    return;
                }
                
                _stopwatch.Stop();
                
                _log.Debug($"errorCode:{response.ErrorCode},response message : {HelloRes.Parser.ParseFrom(response.Data).Message}, {_stopwatch.ElapsedMilliseconds} ms");
            }
            
            
            


            // connector.SendToApi(_apiServicId, new Packet(new CloseSessionMsg()));
            //
            // await Task.Delay(1000);
            //
            //
            // await connector.ConnectAsync("127.0.0.1", 10114);
            //
            //
            // _log.Information("Reconnect");

            // response = await connector.RequestToApi(_apiServicId, new Packet(new AuthenticateReq() { PlatformUid = "10", Token = "passowrd" }));
            //
            // if (!response.IsSuccess())
            // {
            //     _log.Error($"request is not success , error:{response.ErrorCode}");
            //     //Environment.Exit(0);
            // }

            // response = await connector.RequestToApi(_apiServicId, new Packet(new CreateRoomReq() { Data = "success 1" }));
            //
            // if (!response.IsSuccess())
            // {
            //     _log.Error($"request is not success , error:{response.ErrorCode}");
            //     Environment.Exit(0);
            // }
            // //
            // var createRoomRes = CreateRoomRes.Parser.ParseFrom(response.Data);
            // var stageId = createRoomRes.StageId;
            // var playEndpoint = createRoomRes.PlayEndpoint;
            //
            // _log.Information($"CreateRoom: PlayEndpoint:{playEndpoint},StageId:{stageId}");
            // //
            // response = await connector.RequestToApi(_apiServicId, new Packet(new JoinRoomReq() { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 2" }));
            //
            // if (!response.IsSuccess())
            // {
            //     _log.Error($"request is not success , error:{response.ErrorCode}");
            //     Environment.Exit(0);
            // }
            //
            // var joinRoomRes = JoinRoomRes.Parser.ParseFrom(response.Data);
            // var stageIndex = joinRoomRes.StageIdx;
            //
            // _log.Information($"JoinRoomRes: stageIndex:{stageIndex}");
            //
            // response = await connector.RequestToStage(_playServiceId, stageIndex, new Packet(new LeaveRoomReq() { Data = "success 3" }));
            // if (!response.IsSuccess())
            // {
            //     _log.Error($"request is not success , error:{response.ErrorCode}");
            //     Environment.Exit(0);
            // }
            // //
            // response = await connector.RequestToApi(_apiServicId, new Packet(new CreateJoinRoomReq() { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 4" }));
            // if (!response.IsSuccess())
            // {
            //     _log.Error($"request is not success , error:{response.ErrorCode}");
            //     Environment.Exit(0);
            // }
            //connector.SendToStage(_playServiceId, stageIndex, new Packet(new ChatMsg() { Data = "hi!" }));
            //connector.SendToApi(_apiServicId, new Packet(new SendMsg(){Message = "hello!!"} ));
            // response = await connector.RequestToApi(_apiServicId, new Packet(new TestNotRegisterReq() ));
            //  if (!response.IsSuccess())
            //  {
            //      _log.Error($"request is not success , error:{response.ErrorCode}");
            //  }
            //  //
            // response = await connector.RequestToApi(_apiServicId, new Packet(new TestTimeoutReq() ));
            //
            //  if (!response.IsSuccess())
            //  {
            //      _log.Error($"request is not success , error:{response.ErrorCode}");
            //  }
            //  
            //
            await Task.Delay(TimeSpan.FromSeconds(10));
            _log.Information("finish");
            //Environment.Exit(0);

        }
    }
}

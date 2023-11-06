﻿using PlayHouse.Utils;
using PlayHouseConnector;
using Simple;
using Packet = PlayHouseConnector.Packet;

namespace SimpleClient
{
    internal class ClientApplication
    {
        private readonly ushort _apiSvcId = 1;
        private readonly ushort _playSvcId = 2;
        private readonly LOG<ClientApplication> _log = new();
        
        public async Task RunAsync()
        {
            var connector = new Connector(new ConnectorConfig() {RequestTimeout= 3,EnableLoggingResponseTime = true,Host="127.0.0.1",Port=10114});

            connector.OnReceive += (serviceId, packet) =>
            {
               // _log.Information($"message onReceive - [serviceId:{serviceId},msgId:{packet.MsgId}]");

                if (serviceId == _apiSvcId)
                {
                    if(packet.MsgId == SendMsg.Descriptor.Index)
                    {
                        _log.Debug(()=>$"api SendMsg message - [msgId:{packet.MsgId}, message:{SendMsg.Parser.ParseFrom(packet.Data).Message}]");
                    }    
                }
                else if (serviceId == _playSvcId)
                {
                    if(packet.MsgId == ChatMsg.Descriptor.Index) 
                    {
                        _log.Debug(()=>$"stage chat msg -  [msgId:{packet.MsgId},data:{ChatMsg.Parser.ParseFrom(packet.Data).Data}]");
                    }
                }
            };

            connector.OnDisconnect += () =>
            {
                _log.Info(()=>"onDisconnect");
            };

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    connector.MainThreadAction();
                    Thread.Sleep(10);    
                }
            });
            thread.Start();
            //_timer = new Timer((arg) => { connector.MainThreadAction();}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            
            bool result = await connector.ConnectAsync();
            String accountId = Guid.NewGuid().ToString();
            _log.Info(()=>$"onConnect - [accountId:{accountId},result:{result}]");

            try
            {
                var response = await connector.RequestAsync(_apiSvcId,
                    new Packet(new AuthenticateReq() { PlatformUid = accountId, Token = "password" }));

                var authenticateRes = AuthenticateRes.Parser.ParseFrom(response.Data);
                _log.Debug(()=>$"AuthenticateRes - [accountId:{authenticateRes.AccountId},userId:{authenticateRes.UserInfo}]");

                 int offSet = 2;
                
                 for (int i = offSet; i < offSet+10; i++)
                 {
                     if (false == connector.IsConnect())
                     {
                         return;
                     }
                
                     var helloRes = await connector.RequestAsync(_apiSvcId, new Packet(new HelloReq() { Message = "hi!" }));
                
                     // _log.Debug(()=>
                     //     $"response message - [accountId:{accountId},count:{i},message:{HelloRes.Parser.ParseFrom(helloRes.Data).Message}]");
                 }
                
                
                connector.Send(_apiSvcId, new Packet(new CloseSessionMsg()));
                
                await Task.Delay(1000);
            
            
                await connector.ConnectAsync();
                _log.Info(()=>"Reconnect");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                
                 response = await connector.RequestAsync(_apiSvcId, new Packet(new AuthenticateReq() { PlatformUid = accountId, Token = "password" }));
                
                 authenticateRes = AuthenticateRes.Parser.ParseFrom(response.Data);
                 // _log.Info(()=>$"AuthenticateRes - [accountId:{authenticateRes.AccountId},userId:{authenticateRes.UserInfo}]");
                
                for (int i = 0; i < 10; ++i)
                {
                    response = await connector.RequestAsync(_apiSvcId,
                        new Packet(new CreateRoomReq() { Data = "success 1" }));
                
                    var createRoomRes = CreateRoomRes.Parser.ParseFrom(response.Data);
                    var stageId = createRoomRes.StageId;
                    var playEndpoint = createRoomRes.PlayEndpoint;
                
                    // _log.Debug(()=>
                    //     $"CreateRoom - [playEndpoint:{playEndpoint},stageId:{stageId},data:{createRoomRes.Data}]");
                
                    response = await connector.RequestAsync(_apiSvcId,
                        new Packet(new JoinRoomReq()
                            { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 2" }));
                
                
                    var joinRoomRes = JoinRoomRes.Parser.ParseFrom(response.Data);
                    // _log.Debug(()=>
                    //     $"JoinRoomRes - [stageIndex:{joinRoomRes.StageIdx},data:{joinRoomRes.Data}]");
                
                    response = await connector.RequestAsync(_playSvcId,
                        new Packet(new LeaveRoomReq() { Data = "success 3" }));
                    var leaveRoomRes = LeaveRoomRes.Parser.ParseFrom(response.Data);
                
                    // _log.Debug(()=>
                    //     $"LeaveRoomRes - [data:{leaveRoomRes.Data}]");
                
                    //}
                    // var playEndpoint = "tcp://10.12.20.59:10570";
                    // var stageId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray());
                
                    response = await connector.RequestAsync(_apiSvcId,
                        new Packet(new CreateJoinRoomReq()
                            { PlayEndpoint = playEndpoint, StageId = stageId, Data = "success 4" }));
                    
                    var createJoinRoomRes = CreateJoinRoomRes.Parser.ParseFrom(response.Data);
                
                    // _log.Debug(()=>
                    //     $"createJoinRoomRes - [stageIndex:{createJoinRoomRes.StageIdx},data:{createJoinRoomRes.Data}]");
                    
                    connector.Send(_playSvcId, new Packet(new ChatMsg() { Data = "hi!" }));
                    connector.Send(_apiSvcId, new Packet(new SendMsg(){Message = "hello!!"} ));
                    
                    response = await connector.RequestAsync(_playSvcId,
                        new Packet(new LeaveRoomReq() { Data = "success 5" }));
                    
                    var createJoinRoomLeaveRes = LeaveRoomRes.Parser.ParseFrom(response.Data);
                    // _log.Debug(()=>
                    //     $"createJoinRoomLeaveRes - [data:{createJoinRoomLeaveRes.Data}]");
                }
                //
                // try
                // {
                //     await connector.RequestAsync(_apiSvcId, new Packet(new TestNotRegisterReq() ));
                // }
                // catch (PlayConnectorException ex)
                // {
                //     _log.Error(()=>$"Request Error - [accountId:{accountId},{ex.Message}]");
                // }
                //
                // try
                // {
                //     await connector.RequestAsync(_apiSvcId, new Packet(new TestTimeoutReq() ));
                // }
                // catch (PlayConnectorException ex)
                // {
                //     _log.Error(()=>$"Request Error - [accountId:{accountId},{ex.Message}]");
                // }
            
              
            }
            catch (PlayConnectorException ex)
            {
                _log.Error(()=>$"Request Error - [accountId:{accountId},ex:{ex.Message}]");
            }
            catch (Exception ex)
            {
                _log.Error(()=>$"Exception- [accountId:{accountId},ex:{ex}]");
            }
              
            //Environment.Exit(0);
            await Task.Delay(TimeSpan.FromSeconds(5));
            _log.Info(()=>"finish");
            //await _timer.DisposeAsync();
            //Environment.Exit(0);

        }
    }
}

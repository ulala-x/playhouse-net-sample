using System;
using System.Diagnostics;
using NetMQ;
using PlayHouse.Utils;
using PlayHouseConnector;
using Simple;
using Packet = PlayHouseConnector.Packet;

namespace SimpleStress;

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

internal class TestClient
{
    private readonly ushort _apiSvcId = 1;
    private readonly LOG<TestClient> _log = new();
    private readonly ushort _playSvcId = 2;
    private static readonly AtomicShort Sequence = new();
    private readonly Connector _connector = new();
    private long _accountId;
    //private Thread thread;

    public TestClient()
    {
        _connector.Init(new ConnectorConfig
        {
            RequestTimeoutMs = 6000,
            EnableLoggingResponseTime = false,
            Host = "127.0.0.1",
            Port = 10114,
            HeartBeatIntervalMs = 10000,
            ConnectionIdleTimeoutMs = 30000
        });

        //thread = new Thread(() =>
        //{
        //    while (true)
        //    {
        //        _connector.MainThreadAction();
        //        Thread.Sleep(1);
        //    }
        //});
        //thread.Start();

    }

    public async Task RunAsync()
    {
        try
        {

            Random random = new();
            for (int i = 0; i < 10; i++)
            {

                int index = i;



                _connector.Request(_apiSvcId,
                   new Packet(new HelloReq()
                   { Message = CreateMessage(random) })
                   , helloRes =>
                   {
                       _log.Debug(() =>
                           $"response message - [accountId:{_accountId},count:{index},message:{HelloRes.Parser.ParseFrom(helloRes.DataSpan).Message}]");
                   });


                _connector.Request(_apiSvcId,
                    new Packet(new Action_PlayActionReq
                    {
                        Type = i + 10000,
                        Value1 = i + 3000000000,
                        Value2 = i + 4000000000,
                        Value3 = i + 5000000000
                    })
                    , actionRes =>
                    {
                        _log.Debug(() =>
                            $"response message - [accountId:{_accountId},count:{index},message:{Action_PlayActionReq.Parser.ParseFrom(actionRes.DataSpan)}]");
                    });


                //await Task.Delay(10);

            }

            {


                for (int i = 0; i < 10; i++)
                {
                    var helloRes = await _connector.RequestAsync(_apiSvcId,
                        new Packet(new HelloReq()
                            { Message = CreateMessage(random) }));

                    _log.Debug(() =>
                        $"response message - [accountId:{_accountId},,message:{HelloRes.Parser.ParseFrom(helloRes.DataSpan).Message}]");
                }

                

            }
        }
        catch (PlayConnectorException ex)
        {
            _log.Error(() => $"Request Error - [accountId:{_accountId},ex:{ex.Message}]");
        }
        catch (Exception ex)
        {
            _log.Error(() => $"Exception- [accountId:{_accountId},ex:{ex}]");
        }

        //await Task.Delay(TimeSpan.FromSeconds(6));
        //Environment.Exit(0);

        //_log.Info(() => $"finish");
        //await _timer.DisposeAsync();
       // Environment.Exit(0);
    }

    private  string CreateMessage(Random random)
    {
        return "";
        
        //return RandomStringGenerator.GenerateRandomString(random.Next(100, 1000));
    }

    public async Task PrePareAsync()
    {

        try
        {
            string msg = "1234567890qwertyuiop";
            var statusCheckRes = await _connector.RequestAsync((ushort)ServiceId.Session, new Packet(new AccessQueueStatusCheckReq
            {
                Data = msg
            }));
            var accessQueueStatusCheckRes = Simple.AccessQueueStatusCheckRes.Parser.ParseFrom(statusCheckRes.DataSpan);

            _log.Debug(() =>
                $"AccessQueueStatusCheckRes - [AccessQueueStatusCheckRes:{accessQueueStatusCheckRes.Data}]");


            var response = await _connector.AuthenticateAsync(_apiSvcId,
                new Packet(new AuthenticateReq { PlatformUid = _accountId.ToString(), Token = "password" }));


            var authenticateRes = AuthenticateRes.Parser.ParseFrom(response.DataSpan);
            _log.Debug(() =>
                $"AuthenticateRes - [accountId:{authenticateRes.AccountId},userId:{authenticateRes.UserInfo}]");

            _accountId = authenticateRes.AccountId;

        }
        catch (PlayConnectorException ex)
        {
            _log.Error(() => $"Request Error - [accountId:{_accountId},ex:{ex.Message}]");
        }
        catch (Exception ex)
        {
            _log.Error(() => $"Exception- [accountId:{_accountId},ex:{ex}]");
        }

    }

    public void MainThreadAction()
    {
        _connector.MainThreadAction();
    }

    public async Task ConnectAsync()
    {

        var debugMode = false;


        _connector.OnReceive += (serviceId, packet) =>
        {
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

        _connector.OnDisconnect += () => { _log.Info(() => $"onDisconnect"); };

        _connector.OnError += (serviceId, errorCode, request) =>
        {
            _log.Error(() => $"errorCode : {errorCode}, msgId:{request.MsgId}");
        };



        var result = await _connector.ConnectAsync(debugMode);
        _accountId = Sequence.IncrementAndGet();
        _log.Info(() => $"onConnect - [accountId:{_accountId},result:{result}]");

        if (result == false)
        {
            return;
        }

    }
}
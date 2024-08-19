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

internal class ClientApplication
{
    private readonly ushort _apiSvcId = 1;
    private readonly LOG<ClientApplication> _log = new();
    private readonly ushort _playSvcId = 2;
    private static readonly AtomicShort Sequence = new();
    private readonly Connector _connector = new();
    private long _accountId;
    private Thread thread;

    public ClientApplication()
    {
        _connector.Init(new ConnectorConfig
        {
            RequestTimeoutMs = 6000,
            EnableLoggingResponseTime = true,
            Host = "127.0.0.1",
            Port = 10114,
            HeartBeatIntervalMs = 1000,
            ConnectionIdleTimeoutMs = 30000
        });

        thread = new Thread(() =>
        {
            while (true)
            {
                _connector.MainThreadAction();
                Thread.Sleep(1);
            }
        });
        thread.Start();

    }

    public async Task RunAsync()
    {
     
        try
        {

            Random random = new();
            for (int i = 0; i < 100; i++)
            {

                 _connector.Request(_apiSvcId,
                    new Packet(new HelloReq()
                        { Message = CreateMessage(random) })
                    , helloRes =>
                    {
                        _log.Debug(() =>
                            $"response message - [accountId:{_accountId},count:{i},message:{HelloRes.Parser.ParseFrom(helloRes.DataSpan).Message}]");
                    });

                 //await Task.Delay(1000);

            }

            {
                var helloRes = await _connector.RequestAsync(_apiSvcId,
                    new Packet(new HelloReq()
                        { Message = CreateMessage(random) }));

                _log.Debug(() =>
                    $"response message - [accountId:{_accountId},count:last,message:{HelloRes.Parser.ParseFrom(helloRes.DataSpan).Message}]");
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

        await Task.Delay(TimeSpan.FromSeconds(6));
        //Environment.Exit(0);

        _log.Info(() => $"finish");
        //await _timer.DisposeAsync();
        Environment.Exit(0);
    }

    private  string CreateMessage(Random random)
    {
        //return "";
        return RandomStringGenerator.GenerateRandomString(random.Next(100, 1000));
    }

    public async Task PrePareAsync()
    {

        var debugMode = true;
            

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


        var result = await _connector.ConnectAsync(debugMode);
        long accountId = Sequence.IncrementAndGet();
        _log.Info(() => $"onConnect - [accountId:{accountId},result:{result}]");

        if (result == false)
        {
            return;
        }

        try
        {
            var response = await _connector.AuthenticateAsync(_apiSvcId,
                new Packet(new AuthenticateReq { PlatformUid = accountId.ToString(), Token = "password" }));

            var authenticateRes = AuthenticateRes.Parser.ParseFrom(response.DataSpan);
            _log.Debug(() =>
                $"AuthenticateRes - [accountId:{authenticateRes.AccountId},userId:{authenticateRes.UserInfo}]");

            _accountId = authenticateRes.AccountId;

        }
        catch (PlayConnectorException ex)
        {
            _log.Error(() => $"Request Error - [accountId:{accountId},ex:{ex.Message}]");
        }
        catch (Exception ex)
        {
            _log.Error(() => $"Exception- [accountId:{accountId},ex:{ex}]");
        }

    }
}
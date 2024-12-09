using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Production.Api;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Api;
using Serilog;
using Simple;
using SimpleApi.handler;
using SimpleApi.System;
using SimpleProtocol;
using ILogger = Serilog.ILogger;

namespace SimpleApi;

public class ApiApplication
{
    private readonly ILogger _log = Log.Logger;

    public void Run()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddScoped<SampleApiController>();
            services.AddScoped<SampleBackendApiForRoom>();
            services.AddScoped<SampleApiForRoom>();
            services.AddScoped<SimpleApiSystem>();
            services.AddScoped<ISystemController, SimpleApiSystem>();


            _log.Information("api start");
            var commonOption = new PlayhouseOption
            {
                Ip = "127.0.0.1",
                Port = 10470,
                ServiceId = (int)ServiceId.Api,
                RequestTimeoutMSec = 10000,
                ServerId = 0,
                PacketProducer = (msgId, paylaod, msgSeq) => new SimplePacket(msgId, paylaod, msgSeq),
                ServiceProvider = services.BuildServiceProvider()
            };

            var apiOption = new ApiOption();
            var apiServer = new ApiServer(commonOption, apiOption);
            apiServer.Start();

            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                _log.Information("*** shutting down Api server since process is shutting down");
                await apiServer.StopAsync();
                _log.Information("*** server shut down");
                Thread.Sleep(1000);
            };

            _log.Information("Api Server Started");
            apiServer.AwaitTermination();
        }
        catch (Exception ex)
        {
            _log.Error(ex, ex.StackTrace ?? ex.Message);
        }
    }
}
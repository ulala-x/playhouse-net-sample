using PlayHouse.Production.Play;
using PlayHouse.Service.Play;
using Serilog;

namespace SimplePlay.Room;

internal class SimpleUser : IActor
{
    private readonly ILogger _log = Log.Logger;

    public SimpleUser(IActorSender sender)
    {
        ActorSender = sender;
    }

    internal long AccountId => ActorSender.AccountId();
    public IActorSender ActorSender { get; }

    public async Task OnCreate()
    {
        _log.Debug($"OnCreate - [accountId:{ActorSender.AccountId()}]");
        await Task.CompletedTask;
    }

    public async Task OnDestroy()
    {
        _log.Debug($"OnDestroy - [accountId:{ActorSender.AccountId()}]");
        await Task.CompletedTask;
    }
}
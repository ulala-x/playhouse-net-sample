using PlayHouse.Production.Session;
using PlayHouse.Production.Shared;
using Simple;
using SimpleProtocol;

namespace SimpleSession;

class SimpleUser(ISessionSender sender) : ISessionUser
{
    public async  Task OnDispatch(IPacket packet)
    {
        if (packet.MsgId == AccessQueueStatusCheckReq.Descriptor.Name)
        {
            sender.ReplyToClient(new SimplePacket(new AccessQueueStatusCheckRes()));
        }

        await Task.CompletedTask;
    }
}
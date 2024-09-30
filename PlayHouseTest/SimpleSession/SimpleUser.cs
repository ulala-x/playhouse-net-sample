using PlayHouse.Production.Session;
using PlayHouse.Production.Shared;
using Simple;
using SimpleProtocol;

namespace SimpleSession;

class SimpleUser : ISessionUser
{
    public async  Task OnDispatch(IPacket packet, ISessionSender sender)
    {
        if (packet.MsgId == AccessQueueStatusCheckReq.Descriptor.Name)
        {
            sender.ReplyToClient(new SimplePacket(new AccessQueueStatusCheckRes()));
        }

        await Task.CompletedTask;
    }
}
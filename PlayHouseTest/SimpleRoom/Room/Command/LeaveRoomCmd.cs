using PlayHouse.Production.Play;
using PlayHouse.Production.Shared;
using Simple;
using SimpleProtocol;

namespace SimplePlay.Room.Command;

internal class LeaveRoomCmd : IPacketCmd<SimpleRoom, SimpleUser>
{
    public async Task Execute(SimpleRoom room, SimpleUser user, IPacket packet)
    {
        var request = packet.Parse<LeaveRoomReq>();

        user.ActorSender.SendToApi(
            new SimplePacket(new LeaveRoomNotify
                {
                    SessionNid = user.ActorSender.SessionNid(),
                    Sid = user.ActorSender.Sid(),
                    Data = request.Data
                }
            ));

        room.LeaveRoom(user);
        user.ActorSender.LeaveStage();

        room.StageSender.Reply(new SimplePacket(new LeaveRoomRes { Data = request.Data }));

        await Task.CompletedTask;
    }
}
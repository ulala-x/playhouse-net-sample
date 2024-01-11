using PlayHouse.Production;
using PlayHouse.Production.Play;
using Simple;
using SimpleProtocol;
using PlayHouse.Production.Shared;

namespace SimplePlay.Room.Command
{
    internal class LeaveRoomCmd : IPacketCmd<SimpleRoom, SimpleUser>
    {
        public async Task Execute(SimpleRoom room, SimpleUser user, IPacket packet)
        {
            var request = packet.Parse<LeaveRoomReq>();

            user.ActorSender.SendToApi(
                new SimplePacket(new LeaveRoomNotify()
                {
                    SessionEndpoint = user.ActorSender.SessionEndpoint(),
                    Sid = user.ActorSender.Sid(),
                    Data = request.Data
                }
            ));

            room.LeaveRoom(user);
            user.ActorSender.LeaveStage();

            room.StageSender.Reply(new SimplePacket(new LeaveRoomRes() { Data = request.Data}));

            await Task.CompletedTask;


        }
    }
}

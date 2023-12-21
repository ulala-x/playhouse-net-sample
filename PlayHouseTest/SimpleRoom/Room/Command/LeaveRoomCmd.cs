using PlayHouse.Production;
using PlayHouse.Production.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Simple;
using SimpleProtocol;

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

using Playhouse.Simple.Protocol;
using PlayHouse.Production;
using PlayHouse.Production.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlay.Room.Command
{
    internal class LeaveRoomCmd : IPacketCmd<SimpleRoom, SimpleUser>
    {
        public async Task Execute(SimpleRoom room, SimpleUser user, Packet packet)
        {
            var request = LeaveRoomReq.Parser.ParseFrom(packet.Data);

            user.ActorSender.SendToApi(
                new Packet(new LeaveRoomNotify()
                {
                    SessionEndpoint = user.ActorSender.SessionEndpoint(),
                    Sid = user.ActorSender.Sid(),
                    Data = request.Data
                }
            ));

            room.LeaveRoom(user);
            user.ActorSender.LeaveStage();

            room.StageSender.Reply(new ReplyPacket(new LeaveRoomRes() { Data = request.Data}));

            await Task.CompletedTask;


        }
    }
}

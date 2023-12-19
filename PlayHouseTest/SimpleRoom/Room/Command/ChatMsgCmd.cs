using PlayHouse.Production;
using PlayHouse.Production.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;

namespace SimplePlay.Room.Command
{
    internal class ChatMsgCmd : IPacketCmd<SimpleRoom, SimpleUser>
    {
        public async Task Execute(SimpleRoom room, SimpleUser actor, IPacket packet)
        {
            room.SendToAll(packet);
            // room.StageSender.AsyncBlock(
            //     async () => {
            //         await Task.Delay(100);
            //         return "hi";
            //     },
            //     async(arg) =>
            //     {
            //         actor.ActorSender.SendToClient(new Packet(new ChatMsg() { Data = (string)arg }));
            //         room.SendToAll(new Packet(new ChatMsg() { Data = (string)arg }));
            //         await Task.CompletedTask;
            //     }
            // );
            await Task.CompletedTask;
        }
    }
}

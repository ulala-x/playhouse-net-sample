using Google.Protobuf;
using PlayHouse.Communicator.Message;
using PlayHouse.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleProtocol
{
    public class SimpleProtoPayload : IPayload
    {
        private readonly IMessage _proto;

        public SimpleProtoPayload(IMessage proto)
        {
            _proto = proto;
        }

        public IMessage GetProto()
        {
            return _proto;
        }

        public ReadOnlySpan<byte> Data => _proto.ToByteArray();

        public void Dispose()
        {
        }
    }

    public class SimplePacket : IPacket
    {
        private int _msgId;
        private SimpleProtoPayload _payload;

        public SimplePacket(IMessage message)
        {
            _msgId = message.Descriptor.Index;
            _payload =new SimpleProtoPayload(message);
        }

        public int MsgId => _msgId;

        public ReadOnlySpan<byte> Data => _payload.Data;

        public IPayload Payload => _payload;
    }
}

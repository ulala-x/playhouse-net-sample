using Google.Protobuf;
using PlayHouse.Communicator.Message;
using PlayHouse.Production.Shared;

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

        public ReadOnlyMemory<byte> Data => _proto.ToByteArray();


        public void Dispose()
        {
        }
    }

    public static class SimplePacketExtention
    {
        public static T Parse<T>(this IPacket packet) where T : IMessage, new()
        {
            SimplePacket packetObject = (packet as SimplePacket)!;
            return packetObject.Parse<T>();
        }

        public static string GetMsgName(this IPacket packet)
        {
            SimplePacket packetObject = (packet as SimplePacket)!;
            return packetObject.GetMsgName();
        }

        public static IPacket Copy(this IPacket packet)
        {
            SimplePacket packetObject = (packet as SimplePacket)!;
            return new SimplePacket(packet.MsgId, new CopyPayload(packet.Payload), packetObject.MsgSeq);
        }

    }


    public class SimplePacket : IPacket
    {
        private string _msgId;
        private IPayload _payload;
        private IMessage? _parsedMessage;
        private ushort _msgSeq;


        public SimplePacket(IMessage message)
        {
            _msgId = message.Descriptor.Name;
            _payload = new SimpleProtoPayload(message);
            _parsedMessage = message;
        }
        public SimplePacket(string msgId,IPayload payload,ushort msgSeq)
        {
            _msgId = msgId;
            _payload = new CopyPayload(payload);
            _msgSeq = msgSeq;
        }

        public bool IsRequest => _msgSeq > 0;
        public string MsgId => _msgId;
        public IPayload Payload => _payload;
        public ushort MsgSeq => _msgSeq;

        public override string ToString()
        {
            if (_parsedMessage == null)
            {
                //_parsedMessage = ParseMessage();
                return string.Empty;
            }
            return _parsedMessage.ToString() ?? string.Empty;

        }

        public T Parse<T>() where T : IMessage, new()
        {
            if (_parsedMessage == null)
            {
                T message = new T();
                //message.MergeFrom(_Payload.DataSpan);
                //_parsedMessage = message;
                _parsedMessage = message.Descriptor.Parser.ParseFrom(_payload.DataSpan);
                //_parsedMessage = ParseMessage();
            }

            return (T)_parsedMessage;
        }
        public void Dispose()
        {
            _payload.Dispose();
        }
    }
}

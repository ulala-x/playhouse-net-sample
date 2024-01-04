using Google.Protobuf;
using Google.Protobuf.Reflection;
using PlayHouse.Communicator.Message;
using PlayHouse.Production;
using Simple;
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
        private IPayload _Payload;
        private IMessage? _parsedMessage;


        public SimplePacket(IMessage message)
        {
            _msgId = message.Descriptor.Index;
            _Payload = new SimpleProtoPayload(message);
            _parsedMessage = message;
        }
        public SimplePacket(int msgId,IPayload payload)
        {
            _msgId = msgId;
            _Payload = new CopyPayload(payload);
        }


        public int MsgId => _msgId;
        public IPayload Payload => _Payload;

        private IMessage ParseMessage()
        {
            var messageType = SimpleReflection.Descriptor.MessageTypes[MsgId];

            if (messageType == null)
            {
                throw new Exception($"msgId is not invalid - [msgId:{MsgId}]");
            }

            return  messageType.Parser.ParseFrom(_Payload.Data);
        }

        public override string ToString() 
        {
            if( _parsedMessage == null )
            {
                _parsedMessage = ParseMessage();
            }
            return _parsedMessage.ToString() ?? string.Empty     ;

        }

        public T Parse<T>() 
        {
            if (_parsedMessage == null)
            {
                _parsedMessage = ParseMessage();
            }

            return (T) _parsedMessage;
        }

        public IPacket Copy()
        {
            return new SimplePacket(_msgId, _Payload);
        }
    }
}

﻿using Google.Protobuf;
using PlayHouse.Communicator.Message;
using PlayHouse.Production.Shared;
using Simple;

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


    public class SimplePacket : IPacket
    {
        private int _msgId;
        private IPayload _payload;
        private IMessage? _parsedMessage;
        private ushort _msgSeq;


        public SimplePacket(IMessage message)
        {
            _msgId = message.Descriptor.Index;
            _payload = new SimpleProtoPayload(message);
            _parsedMessage = message;
        }
        public SimplePacket(int msgId,IPayload payload,ushort msgSeq)
        {
            _msgId = msgId;
            _payload = new CopyPayload(payload);
            _msgSeq = msgSeq;
        }

        public bool IsRequest => _msgSeq > 0;
        public int MsgId => _msgId;
        public IPayload Payload => _payload;

        private IMessage ParseMessage()
        {
            var messageType = SimpleReflection.Descriptor.MessageTypes[MsgId];

            if (messageType == null)
            {
                throw new Exception($"msgId is not invalid - [msgId:{MsgId}]");
            }

            return  messageType.Parser.ParseFrom(_payload.DataSpan);
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
            return new SimplePacket(_msgId, new CopyPayload(_payload),_msgSeq);
        }

        public void Dispose()
        {
            _payload.Dispose();
        }
    }
}

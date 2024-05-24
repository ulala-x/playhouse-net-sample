using Google.Protobuf;
using PlayHouse.Communicator.Message;
using PlayHouse.Production.Shared;

namespace SimpleProtocol;

public class SimpleProtoPayload : IPayload
{
    private readonly IMessage _proto;

    public SimpleProtoPayload(IMessage proto)
    {
        _proto = proto;
    }

    public ReadOnlyMemory<byte> Data => _proto.ToByteArray();


    public void Dispose()
    {
    }

    public IMessage GetProto()
    {
        return _proto;
    }
}

public static class SimplePacketExtention
{
    public static T Parse<T>(this IPacket packet) where T : IMessage, new()
    {
        var packetObject = (packet as SimplePacket)!;
        return packetObject.Parse<T>();
    }

    public static string GetMsgName(this IPacket packet)
    {
        var packetObject = (packet as SimplePacket)!;
        return packetObject.GetMsgName();
    }

    public static IPacket Copy(this IPacket packet)
    {
        var packetObject = (packet as SimplePacket)!;
        return new SimplePacket(packet.MsgId, new CopyPayload(packet.Payload), packetObject.MsgSeq);
    }
}

public class SimplePacket : IPacket
{
    private IMessage? _parsedMessage;


    public SimplePacket(IMessage message)
    {
        MsgId = message.Descriptor.Index;
        Payload = new SimpleProtoPayload(message);
        _parsedMessage = message;
    }

    public SimplePacket(int msgId, IPayload payload, ushort msgSeq)
    {
        MsgId = msgId;
        Payload = new CopyPayload(payload);
        MsgSeq = msgSeq;
    }

    public bool IsRequest => MsgSeq > 0;
    public ushort MsgSeq { get; }

    public int MsgId { get; }

    public IPayload Payload { get; }

    public void Dispose()
    {
        Payload.Dispose();
    }

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
            var message = new T();
            //message.MergeFrom(_Payload.DataSpan);
            //_parsedMessage = message;
            _parsedMessage = message.Descriptor.Parser.ParseFrom(Payload.DataSpan);
            //_parsedMessage = ParseMessage();
        }

        return (T)_parsedMessage;
    }
}
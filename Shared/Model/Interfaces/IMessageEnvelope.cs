using System;

namespace HomeControl.Hub.Handlers
{
    public interface IMessageEnvelope
    {
        byte[] SenderIdentity { get; set; }

        Type MessageBodyType { get; set; }

        IMessage Message { get; set; }
    }
}
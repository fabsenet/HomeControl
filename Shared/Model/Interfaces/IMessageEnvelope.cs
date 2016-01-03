using System;

namespace HomeControl.Shared.Model.Interfaces
{
    public interface IMessageEnvelope
    {
        byte[] SenderIdentity { get; set; }

        Type MessageBodyType { get; set; }

        IMessage Message { get; set; }
    }
}
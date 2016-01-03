using HomeControl.Shared.Model.Interfaces;
using NetMQ;
using Raven.Imports.Newtonsoft.Json;

namespace HomeControl.Hub.Util
{

    public static class MessageExtensions
    {
        public static NetMQMessage ToMqMessage(this IMessage message, byte[] receiverIdentity)
        {
            var mqMessage = new NetMQMessage();
            mqMessage.Append(receiverIdentity);
            mqMessage.Append(message.GetType().Name);
            mqMessage.Append(JsonConvert.SerializeObject(message, Formatting.None));

            return mqMessage;
        }
    }
}
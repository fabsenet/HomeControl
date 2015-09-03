using System.Threading;
using Amqp;
using Amqp.Framing;
using Amqp.Sasl;

namespace IotActor
{
    public class AmqpEndpointController
    {
        readonly Connection _connection;
        readonly Session _session;
        readonly SenderLink _sender;
        readonly ReceiverLink _receiver;

        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private readonly LedOnOffSwitch _led;

        public AmqpEndpointController()
        {
            _led = new LedOnOffSwitch(18, _token.Token);

            _connection = new Connection(new Address("amqp://guest:guest@xenon:5672"));
            _session = new Session(_connection);
            _sender = new SenderLink(_session, "send-link", "control");
            _receiver = new ReceiverLink(_session, "recv-link", "data");
            _receiver.Start(50, OnMessage);
        }

        private void OnMessage(ReceiverLink receiverLink, Message message)
        {
            var shouldBeOn = (string) message.ApplicationProperties["Light"] == "on";
            _led.SetState(shouldBeOn);
        }
    }

}
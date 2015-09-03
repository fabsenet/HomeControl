using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;

namespace IotActor
{
    public class LedOnOffSwitch
    {
        private readonly Task _internalRunner;

        private readonly ConcurrentQueue<Action<GpioPin>> _actions = new ConcurrentQueue<Action<GpioPin>>(); 

        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

        public LedOnOffSwitch(int pinNumber, CancellationToken token)
        {
            _internalRunner = Task.Run(() => Run(pinNumber, token));

        }

        public void SetState(bool on)
        {
            EnqueueAction(pin => pin.Write(on ? GpioPinValue.High : GpioPinValue.Low));
        }

        private void EnqueueAction(Action<GpioPin> action)
        {
            _actions.Enqueue(action);
            _resetEvent.Set();
        }

        private void Run(int pinNumber, CancellationToken token)
        {
            //init led
            var pin = GpioController.GetDefault().OpenPin(pinNumber, GpioSharingMode.Exclusive);
            pin.Write(GpioPinValue.Low);
            pin.SetDriveMode(GpioPinDriveMode.Output);
            do
            {
                Action<GpioPin> action;
                if (_actions.TryDequeue(out action))
                {
                    action(pin);
                }

                _resetEvent.Wait(token);

            } while (!token.IsCancellationRequested);

            pin.Write(GpioPinValue.Low);
            pin.SetDriveMode(GpioPinDriveMode.Input);
        }
    }
}
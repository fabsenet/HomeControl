using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using IotActor.Annotations;

namespace IotActor
{
    public class LedOnOffSwitch : INotifyPropertyChanged, ILedOnOffSwitch
    {
        private readonly CoreDispatcher _propertyChangedDispatcher;
        [UsedImplicitly] private readonly Task _internalRunner;

        private readonly BlockingCollection<Func<bool>> _actions = new BlockingCollection<Func<bool>>(); 

        private GpioPin _pin;
        private bool _isOn;
        private bool _isActuallyBound;

        public LedOnOffSwitch(int pinNumber, CancellationToken token, CoreDispatcher propertyChangedDispatcher = null)
        {
            _propertyChangedDispatcher = propertyChangedDispatcher;
            PinNumber = pinNumber;
            _internalRunner = Task.Run(() => Run(pinNumber, token));
        }

        public bool IsOn
        {
            get { return _isOn; }
            private set
            {
                var gpioPinValue = value?GpioPinValue.High : GpioPinValue.Low;
                _pin?.Write(gpioPinValue);
                Debug.WriteLine("switching GPIO pin {0} to {1}", PinNumber, gpioPinValue);
                _isOn = value;
                OnPropertyChanged();
            }
        }

        public int PinNumber { get; }

        public bool IsActuallyBound
        {
            get { return _isActuallyBound; }
            private set
            {
                _isActuallyBound = value;
                OnPropertyChanged();
            }
        }

        public void SetState(bool on) => _actions.Add(() => @on);

        private void Run(int pinNumber, CancellationToken token)
        {
            //init led pin
            try
            {
                //if there is no gpio it returns null.
                var gpioController = GpioController.GetDefault();
                _pin = gpioController?.OpenPin(pinNumber, GpioSharingMode.Exclusive);
            }
            catch
            {
                // ignored
                //running in x86 mode yields FileNotFoundException but in this case, all we need to know is, 
                //there is no hardware gpio!

            }

            IsActuallyBound = _pin != null;
            if (_pin != null)
            {
                _pin.Write(GpioPinValue.Low);
                _pin.SetDriveMode(GpioPinDriveMode.Output);
            }
            do
            {
                Func<bool> action;
                if (_actions.TryTake(out action))
                {
                    IsOn = action();
                }

            } while (!token.IsCancellationRequested);

            if (_pin != null)
            {
                _pin.Write(GpioPinValue.Low);
                _pin.SetDriveMode(GpioPinDriveMode.Input);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_propertyChangedDispatcher == null || _propertyChangedDispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                await _propertyChangedDispatcher.RunAsync(CoreDispatcherPriority.Normal, 
                    () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}
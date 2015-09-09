using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace IotActor
{
    public sealed partial class MainPage : Page
    {
        private readonly AmqpEndpointController _controller;

        private readonly ObservableCollection<LedOnOffSwitch> _ledOnOffSwitches = new ObservableCollection<LedOnOffSwitch>();
        private Task _ledSwitchingTask;

        public MainPage()
        {
            this.InitializeComponent();

            //_controller = new AmqpEndpointController();
            DataContext = _ledOnOffSwitches;
            var token = new CancellationTokenSource();
            var led = new LedOnOffSwitch(18, token.Token, Dispatcher);
            _ledOnOffSwitches.Add(led);
            _ledSwitchingTask = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    led.SetState(i%2 == 0);
                    Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                }
                for (int i = 0; i < 4; i++)
                {
                    led.SetState(i%2 == 0);
                    Task.Delay(TimeSpan.FromMilliseconds(5000)).Wait();
                }
            });
        }

    }
}

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
        private CancellationTokenSource _token;

        public MainPage()
        {
            this.InitializeComponent();

            //_controller = new AmqpEndpointController();


            DataContext = _ledOnOffSwitches;
            _token = new CancellationTokenSource();
            var led = new LedOnOffSwitch(18, _token.Token, Dispatcher);
            _ledOnOffSwitches.Add(led);
            _ledSwitchingTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        led.SetState(i%2 == 0);
                        Task.Delay(TimeSpan.FromMilliseconds(300)).Wait();
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        led.SetState(i%2 == 0);
                        Task.Delay(TimeSpan.FromMilliseconds(3000)).Wait();
                    }
                }
            }, _token.Token);
        }

    }
}

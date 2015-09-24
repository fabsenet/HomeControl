using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace IotActor
{
    public sealed partial class MainPage : Page
    {
        private SignalrClientEndpointController _controller;

        private readonly ObservableCollection<LedOnOffSwitch> _ledOnOffSwitches = new ObservableCollection<LedOnOffSwitch>();
        private readonly Task _communicationsTask;

        public MainPage()
        {
            this.InitializeComponent();

            _communicationsTask = Task.Run(() => _controller = new SignalrClientEndpointController(_ledOnOffSwitches, Dispatcher));
            
            DataContext = _ledOnOffSwitches;
        }

    }
}

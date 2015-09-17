namespace HomeControl.Shared.Model
{
    public class LedOnOffSwitchConfiguration
    {
        public int PinNumber { get; set; }

        /// <summary>
        /// true means on / high
        /// false means off / low
        /// </summary>
        public bool InitialState { get; set; }
    }
}
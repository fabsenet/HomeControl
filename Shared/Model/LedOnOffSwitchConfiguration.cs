namespace HomeControl.Shared.Model
{
    public class LedOnOffSwitchConfiguration
    {
        /// <summary>
        /// The name of the LED or its location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The used pin where the led is connected
        /// </summary>
        public int PinNumber { get; set; }

        /// <summary>
        /// true means on / high
        /// false means off / low
        /// </summary>
        public bool InitialState { get; set; }
    }
}
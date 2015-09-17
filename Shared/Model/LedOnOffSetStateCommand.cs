namespace HomeControl.Shared.Model
{
    public class LedOnOffSetStateCommand
    {
        public int PinNumber { get; set; }

        /// <summary>
        /// true means on / high
        /// false means off / low
        /// </summary>
        public bool DesiredState { get; set; }

    }
}
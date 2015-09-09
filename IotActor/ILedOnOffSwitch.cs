namespace IotActor
{
    public interface ILedOnOffSwitch
    {
        bool IsOn { get; }
        bool IsActuallyBound { get; }
        int PinNumber { get; }
    }

    public class SampleLedOnOffSwitch : ILedOnOffSwitch
    {
        public bool IsOn { get; set; }
        public bool IsActuallyBound { get; set; }
        public int PinNumber { get; set; }
    }
}
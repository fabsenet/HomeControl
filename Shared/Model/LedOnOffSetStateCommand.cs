using HomeControl.Shared.Model.Interfaces;

namespace HomeControl.Shared.Model
{
    public class LedOnOffSetStateCommand : IMessage
    {
        public int PinNumber { get; set; }

        /// <summary>
        /// Represents the state of the output/LED:
        /// 0.0f => off
        /// 1.0f => on
        /// 0.0..1.0 => dimmed
        /// </summary>
        public float Value { get; set; }

    }
}
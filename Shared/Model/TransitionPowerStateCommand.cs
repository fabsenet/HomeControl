using HomeControl.Shared.Model.Interfaces;

namespace HomeControl.Shared.Model
{
    public class TransitionPowerStateCommand : IMessage
    {
        public PowerStateEnum DesiredPowerState { get; set; }
    }
}
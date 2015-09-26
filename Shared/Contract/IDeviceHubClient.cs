namespace HomeControl.Shared.Contract
{
    public interface IDeviceHubClient
    {
        void Configure(string command);
        void LedOnOffSetStateCommand(string command);
    }

    public interface IDeviceHubServer
    {
        
    }
}
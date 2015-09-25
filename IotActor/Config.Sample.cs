namespace IotActor
{
    public static class Config
    {
        /// <summary>
        /// The URL where the signalr endpoint of the web project is running
        /// </summary>
        public const string SignalrHubUrl = "https://someserver/signalr";

        /// <summary>
        /// The user name if your endpoint have (windows) authentication enabled or null
        /// </summary>
        public const string SignalrHubUserName = "someUser";

        /// <summary>
        /// The password for the user account or null, if your signalr hub has no authentication
        /// </summary>
        public const string SignalrHubPassword = "somePassword";
    }
}
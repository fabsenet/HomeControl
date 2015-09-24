using System;

namespace HomeControl.Shared.Model
{
    public class PingResponse
    {
        public DateTime PingTime { get; set; } 
        public string HostName { get; set; } 
    }
}
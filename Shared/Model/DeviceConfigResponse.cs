using System;
using System.Collections.Generic;

namespace HomeControl.Shared.Model
{
    public class DeviceConfigResponse
    {
        public DeviceConfigResponse()
        {
            LedOnOffSwitches = new List<LedOnOffSwitchConfiguration>();
        }
        public List<LedOnOffSwitchConfiguration> LedOnOffSwitches { get; private set; }
        public TimeSpan? ApplicationlevelPingTimeSpan { get; set; }
    }
}
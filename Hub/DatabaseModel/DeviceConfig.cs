using System;
using System.Collections.Generic;
using HomeControl.Shared.Model;
using Raven.Imports.Newtonsoft.Json;

namespace HomeControl.Hub.DatabaseModel
{
    public class DeviceConfig
    {
        public string Id { get; set; }

        public string Hostname { get; set; }

        public DateTime LastOnlineTime { get; set; }

        public bool IsCurrentlyOnline { get; set; }

        [JsonIgnore]
        public bool ConsideredOnline => IsCurrentlyOnline && LastOnlineTime + TimeSpan.FromMinutes(3) > DateTime.UtcNow;

        public Dictionary<int,bool> LedStatesByPinNumber { get; set; }=new Dictionary<int, bool>();

        public List<LedOnOffSwitchConfiguration> LedOnOffSwitchConfigurations { get; set; } = new List<LedOnOffSwitchConfiguration>();

        public bool PowerStateControllable { get; set; }

        public TimeSpan? ApplicationlevelPingTimeSpan { get; set; }
    }
}
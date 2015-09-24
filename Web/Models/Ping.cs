using System;
using Raven.Imports.Newtonsoft.Json;

namespace Web.Models
{
    public class Ping
    {
        public string Id { get; set; }

        public string Hostname { get; set; }

        public DateTime LastOnlineTime { get; set; }

        public bool IsCurrentlyOnline { get; set; }

        [JsonIgnore]
        public bool ConsideredOnline => IsCurrentlyOnline && LastOnlineTime + TimeSpan.FromMinutes(10) > DateTime.UtcNow;
    }
}
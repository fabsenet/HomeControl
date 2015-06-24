using System;
using Raven.Imports.Newtonsoft.Json;

namespace Web.Models
{
    public class Ping
    {
        public string Id { get; set; }

        public string Hostname { get; set; }

        public DateTime LastOnlineTime { get; set; }

        [JsonIgnore]
        public bool ConsideredOnline => LastOnlineTime + TimeSpan.FromMinutes(3) > DateTime.UtcNow;
    }
}
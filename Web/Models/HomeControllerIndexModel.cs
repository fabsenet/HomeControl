using System;
using System.Collections;
using System.Collections.Generic;
using HomeControl.Shared.Model;

namespace Web.Models
{
    public class HomeControllerIndexModel
    {
        public IList<DeviceConfig> Pings { get; set; }
    }
}
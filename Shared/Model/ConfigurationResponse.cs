using System;
using System.Collections.Generic;
using HomeControl.Shared.Model.Interfaces;

namespace HomeControl.Shared.Model
{
    public class ConfigurationResponse : IMessage
    {
        public ConfigurationResponse()
        {
            GpioPins = new List<GpioPin>();
        }

        public List<GpioPin> GpioPins { get; private set; }
    }

    public enum GpioPinTypeEnum
    {
        PwmOut,
        Input,
    }

    public class GpioPin
    {
        public GpioPinTypeEnum Type { get; set; }
        public int PinNumber { get; set; }
        public string Name { get; set; }
        public int? Frequency { get; set; }
        public float? InitialValue { get; set; }
    }
}
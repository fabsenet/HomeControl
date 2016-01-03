using System;
using HomeControl.Shared.Model.Interfaces;

namespace HomeControl.Shared.Model
{
    public class InputChangedTelemetry : IMessage
    {
        public string Hostname { get; set; }
        public DateTime CreateDate { get; set; }

        public int PinNumber { get; set; }
        public string Name { get; set; }
        public float NewValue { get; set; }
    }
}
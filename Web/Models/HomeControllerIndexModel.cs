using System;
using System.Collections;
using System.Collections.Generic;

namespace Web.Models
{
    public class HomeControllerIndexModel
    {
        public IList<Ping> Pings { get; set; }
    }
}
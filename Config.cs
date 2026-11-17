using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace CustomOverlayFramework
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;

        
        [Description("The adress of the opened connection")]
        public string ListenAddress { get; set; } = "127.0.0.1";

        [Description("Open port to communicate with the overlay")]
        public int Port { get; set; } = 7790;

        [Description("Private token, to authenticate the overlay")]
        public string Token { get; set; } = "changeme";

        [Description("Blacklisted user from the overlay (empty for nobody)")]
        public List<string> BlacklistedUserId { get; set; } = new List<string>();

        [Description("Check made every tick, does not send to the overlay every tick it's internal")]
        public float TicksPerSecond { get; set; } = 20f;
    }
}

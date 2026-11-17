using Exiled.API.Enums;
using Exiled.API.Features;
using System;

namespace CustomOverlayFramework
{
    public class MainPlugin : Plugin<Config>
    {
        public static MainPlugin Instance { get; private set; }
        public override string Name => "CustomOverlayFramework";
        public override string Author => "Kruacent Team";
        public override string Prefix => "COFramework";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 15);
        public override PluginPriority Priority => PluginPriority.Higher;

        public override void OnEnabled()
        {
            Instance = this;
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Instance = null;
            base.OnDisabled();
        }


    }
}

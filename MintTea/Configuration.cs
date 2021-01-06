using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MintTea {
    class Configuration {

        public static ConfigEntry<float> MaxAirAccel { get; set; }
        public static ConfigEntry<float> AirAccel { get; set; }

        public static void InitConfig(ConfigFile configFile) {
            MaxAirAccel = configFile.Bind(
                "Quake",
                "Max air acceleration",
                3f,
                "I don't know what this number does. Default: 3"
            );

            AirAccel = configFile.Bind(
                "Quake",
                "Air acceleration",
                30f,
                "Higher values mean more speedgain while strafing. Default: 30"
            );
        }
    }
}

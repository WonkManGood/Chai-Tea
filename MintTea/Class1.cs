using BepInEx;

namespace MyUserName {
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.wellme.MintTea", "Mint tea", "0.0.0")]
    public class MintTea : BaseUnityPlugin {
        public void Awake() {
            Logger.LogMessage("Loaded MyModName!");
        }
    }
}
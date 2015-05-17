using Rocket.API;


namespace ApokPT.RocketPlugins
{
    public class AntiKOSConfiguration : IRocketPluginConfiguration
    {

        public bool Enabled;
        public byte MinimumItems;
        public byte MinimumContainers;
        public bool NoEquipedWeapons;
        public byte MaxWarnings;
        public bool ResetWarningsAfterExecute;

        public IRocketPluginConfiguration DefaultConfiguration
        {
            get
            {
                AntiKOSConfiguration config = new AntiKOSConfiguration();
                config.Enabled = true;
                config.MinimumItems = 5;
                config.MinimumContainers = 0;
                config.NoEquipedWeapons = true;
                config.ResetWarningsAfterExecute = true;
                config.MaxWarnings = 2;
                return config;
            }
        }
    }
}

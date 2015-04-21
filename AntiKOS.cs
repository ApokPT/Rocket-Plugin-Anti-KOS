using Rocket.Logging;
using Rocket.RocketAPI;
using Rocket.RocketAPI.Events;
using SDG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApokPT.RocketPlugins
{

    class AntiKOS : RocketPlugin<AntiKOSConfiguration>
    {

        public static AntiKOS Instance;

        private static Dictionary<string, byte> Warnings = new Dictionary<string, byte>();

        protected override void Load()
        {
            Instance = this;
            if (!Configuration.Enabled) return;
            Rocket.RocketAPI.Events.RocketPlayerEvents.OnPlayerDeath += RocketPlayerEvents_OnPlayerDeath;
        }
                
        private void RocketPlayerEvents_OnPlayerDeath(RocketPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            switch (cause)
            {
                case EDeathCause.GUN:
                case EDeathCause.KILL:
                case EDeathCause.ROADKILL:
                    break;
                default:
                    return;
            }

            RocketPlayer killer = RocketPlayer.FromCSteamID(murderer);
            if (killer.Permissions.Contains("anti-kos.imune")) return;

            if (player.ToString() == killer.ToString()) return;

            bool mainWeapon = player.Inventory.Items[(int)(InventoryGroup.Primary)].getItemCount() == 1;
            bool offhandWeapon = player.Inventory.Items[(int)(InventoryGroup.Secondary)].getItemCount() == 1;

            bool weaponInHands = player.Player.Equipment.HoldingItemID != 0;

            ushort itemCount = 0;
            byte containersCount = 0;

            for (ushort i = 0; i < player.Inventory.Items.Length; i++)
            {
                itemCount += player.Inventory.Items[i].getItemCount();
                if (i > 2 && player.Inventory.Items[i].SizeX != 0) containersCount += 1;
            }


            if (Configuration.NoEquipedWeapons && (!mainWeapon && !offhandWeapon || !weaponInHands))
            {
                Logger.Log("KOS LOG: " + killer.CharacterName + " killed unarmed " + player.CharacterName + "!");
                CheckWarnings(player, killer);
                return;
            }

            if (itemCount <= Configuration.MinimumItems)
            {
                Logger.Log("KOS LOG: " + killer.CharacterName + " killed " + player.CharacterName + " with only " + itemCount + " item(s)!");
                CheckWarnings(player, killer);
                return;
            }

            if (containersCount <= Configuration.MinimumContainers)
            {
                Logger.Log("KOS LOG: " + killer.CharacterName + " killed " + player.CharacterName + " with only " + containersCount + " containers(s)!");
                CheckWarnings(player, killer);
                return;
            }

            

        }

        private void CheckWarnings(RocketPlayer player, RocketPlayer killer)
        {
            string klStr = killer.ToString();

            if (Configuration.MaxWarnings - 1 > 0)
            {
                if (Warnings.ContainsKey(killer.ToString()))
                {

                    byte killerWarnings = Warnings[klStr];

                    if (killerWarnings == Configuration.MaxWarnings - 1)
                    {
                        if (Configuration.ResetWarningsAfterExecute)
                            Warnings.Remove(klStr);
                            Execute(player, killer);
                    }
                    else if (killerWarnings + 1 == Configuration.MaxWarnings - 1)
                    {
                        Warnings[klStr] += 1;
                        Logger.Log(killer.CharacterName + " warned " + Warnings[klStr] + "/" + Configuration.MaxWarnings + " time(s) for KOS!");
                        RocketChatManager.Say(killer, AntiKOS.Instance.Translate("antikos_warning_last"));
                    }
                    else
                    {
                        Warnings[klStr] += 1;
                        Logger.Log(killer.CharacterName + " warned " + Warnings[klStr] + "/" + Configuration.MaxWarnings + " time(s) for KOS!");
                        RocketChatManager.Say(killer, AntiKOS.Instance.Translate("antikos_warning", Warnings[klStr], Configuration.MaxWarnings));
                        
                    }
                }
                else
                {
                    Warnings.Add(klStr, 1);
                    Logger.Log(killer.CharacterName + " warned " + Warnings[klStr] + "/" + Configuration.MaxWarnings + " time(s) for KOS!");
                    RocketChatManager.Say(killer, AntiKOS.Instance.Translate("antikos_warning", Warnings[klStr], Configuration.MaxWarnings));
                }
            }
            else
            {
                Execute(player, killer);
            }
        }

        private void Execute(RocketPlayer player, RocketPlayer killer)
        {
            Logger.Log(killer.CharacterName + " was executed for KOS!");
            RocketChatManager.Say(AntiKOS.Instance.Translate("antikos_executed", killer.CharacterName));
            RocketChatManager.Say(player, AntiKOS.Instance.Translate("antikos_execute"));
            killer.Damage(255, player.Position, EDeathCause.PUNCH, ELimb.SKULL, killer.CSteamID);
        }

        // Translations

        public override Dictionary<string, string> DefaultTranslations
        {
            get
            {
                return new Dictionary<string, string>(){
                    {"antikos_warning","ANTI-KOS : This is your {0}# warning of {1}!"},
                    {"antikos_warning_last","ANTI-KOS : This is your last warning, next KOS will kill you!"},
                    {"antikos_executed","ANTI-KOS : {0} have been executed for KOS!"},
                    {"antikos_execute","ANTI-KOS : Your killer has been executed for KOS!"}
                };
            }
        }
    }
}

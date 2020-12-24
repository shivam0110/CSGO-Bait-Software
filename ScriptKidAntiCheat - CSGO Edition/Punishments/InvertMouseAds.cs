﻿using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Timers;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: InvertMouseAds
     DESCRIPTION: Reverse mouse while ADS
    */
    class InvertMouseAds : Punishment
    {
        public bool InvertActivated = false;

        public override int ActivateOnRound { get; set; } = 2;

        public InvertMouseAds() : base(0) // 0 = Always active
        {

        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (Program.GameData.MatchInfo.isFreezeTime) return;

                Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                if (Player.isAimingDownScope == true && (ActiveWeapon == Weapons.Awp || ActiveWeapon == Weapons.Scout || ActiveWeapon == Weapons.Sig || ActiveWeapon == Weapons.Scar))
                {
                    if (InvertActivated == false)
                    {
                        ActivatePunishment();
                    }
                }
                else if (InvertActivated == true)
                {
                    Reset();
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "InvertMouseAdsException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            InvertActivated = true;

            Program.GameConsole.SendCommand("m_pitch -0.022");

            base.AfterActivate();
        }

        override public void Reset()
        {
            InvertActivated = false;
            Program.GameConsole.SendCommand("m_pitch +0.022");
        }
    }
}

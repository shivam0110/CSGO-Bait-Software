using ScriptKidAntiCheat.Internal;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Utils.Maths;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: BloodBrothers
     DESCRIPTION: Automatically fire the cheaters weapon when crosshair enters the friendly hitbox
    */
    class BloodBrothers : Punishment
    {

        public BloodBrothers() : base(0, false, 100) // 0 = Always active
        {
            // THIS PUNISHMENT HAS BEEN REMOVED FROM THE PUBLIC SOURCE CODE - CHECK README.TXT FOR MORE INFO
        }

    }
}

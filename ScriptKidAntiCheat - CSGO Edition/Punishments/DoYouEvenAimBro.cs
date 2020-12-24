using ScriptKidAntiCheat.Internal;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Utils.Maths;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static ScriptKidAntiCheat.Utils.MouseHook;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: DoYouEvenAimBro
     DESCRIPTION: Cause the cheater to look straight up into the sky when crosshair enters enemy hitbox
    */
    class DoYouEvenAimBro : Punishment
    {

        public DoYouEvenAimBro() : base(0, false, 100) // 0 = Always active
        {
            // THIS PUNISHMENT HAS BEEN REMOVED FROM THE PUBLIC SOURCE CODE - CHECK README.TXT FOR MORE INFO
        }
    }
}

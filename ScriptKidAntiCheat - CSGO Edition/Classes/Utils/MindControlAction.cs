using ScriptKidAntiCheat.Punishments;
using SharpDX;

namespace ScriptKidAntiCheat.Utils
{
    class MindControlAction
    {
        public Vector3 AimLockAtWorldPoint { get; set; }
        public int AimLockDuration { get; set; } = 3000;
        public string ConsoleCommand { get; set; } = "";
        public int Sleep { get; set; } = 0;
    }
}

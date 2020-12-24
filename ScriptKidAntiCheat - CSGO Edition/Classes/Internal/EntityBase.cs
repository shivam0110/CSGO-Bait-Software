using System;
using ScriptKidAntiCheat.Data;
using ScriptKidAntiCheat.Utils;
using SharpDX;

/*
 * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
 */
namespace ScriptKidAntiCheat.Internal
{
    public abstract class EntityBase
    {
        public IntPtr AddressBase { get; protected set; }

        public bool LifeState { get; protected set; }

        public int Health { get; protected set; }

        public Team Team { get; protected set; }

        public Vector3 Origin { get; private set; }

        public virtual bool IsAlive()
        {
            return AddressBase != IntPtr.Zero &&
                   !LifeState &&
                   Health > 0 &&
                   (Team == Team.Terrorists || Team == Team.CounterTerrorists);
        }
        protected abstract IntPtr ReadAddressBase(GameProcess gameProcess);
        public virtual bool Update(GameProcess gameProcess)
        {
            AddressBase = ReadAddressBase(gameProcess);
            if (AddressBase == IntPtr.Zero)
            {
                return false;
            }

            LifeState = gameProcess.Process.Read<bool>(AddressBase + Offsets.m_lifeState);
            Health = gameProcess.Process.Read<int>(AddressBase + Offsets.m_iHealth);
            Team = (Team)gameProcess.Process.Read<int>(AddressBase + Offsets.m_iTeamNum);
            Origin = gameProcess.Process.Read<Vector3>(AddressBase + Offsets.m_vecOrigin);

            return true;
        }
    }
}

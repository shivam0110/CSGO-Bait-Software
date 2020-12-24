using ScriptKidAntiCheat.Data;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Utils.Maths;
using SharpDX;
using System;
using System.Text;

/*
 * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
 */
namespace ScriptKidAntiCheat.Internal
{
    public class Player : EntityBase
    {
        public Vector3 ViewOffset { get; private set; }
        public Vector3 EyePosition { get; private set; }
        public Vector3 EyeDirection { get; private set; }
        public Vector3 ViewAngles { get; private set; }
        public Vector3 AimPunchAngle { get; private set; }
        public Vector3 AimDirection { get; private set; }
        public Vector3 vecVelocity { get; private set; }
        public int PlayerIndex { get; set; } = 0;
        public int Fov { get; private set; }
        public int ClientState { get; private set; }

        public static double weapon_recoil_scale = 2.0f;
        public short ActiveWeapon { get; private set; }
        public short PrimaryWeapon { get; set; }
        public int AmmoCount { get; private set; }
        public bool HasDefuseKit { get; private set; } = false;
        public bool CanShoot { get; private set; } = false;
        public bool CursorLocked { get; private set; } = false;

        public bool isAimingDownScope = false;
        public string Location { get; set; }
        public string SpawnLocationName { get; set; }
        public string NickName { get; set; } = null;

        protected override IntPtr ReadAddressBase(GameProcess gameProcess)
        {
            return gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwLocalPlayer);
        }

        public short getActiveWeapon(GameProcess gameProcess)
        {
            int activeWeapon = gameProcess.Process.Read<int>(AddressBase + Offsets.m_hActiveWeapon) & 0xfff;
            IntPtr weaponEntity = gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwEntityList + (activeWeapon - 1) * 0x10);
            short weaponID = gameProcess.Process.Read<short>(weaponEntity + Offsets.m_iItemDefinitionIndex);

            return weaponID;
        }

        public string getPlayerLocation(GameProcess gameProcess)
        {
            string locname = MemoryRead.ReadString(gameProcess.ModuleClient, AddressBase, Offsets.m_szLastPlaceName, 18);
            return locname;
        }

        public void UpdatePrimaryWeapon(short ActiveWeapon)
        {
            // Should probably read this from memory but this is good enough for now :D
            if (PrimaryWeapon != ActiveWeapon)
            {
                // Update primary whenever primary weapon is active (only includes "normal" buyround primaries)
                Weapons CurrWeapon = (Weapons)ActiveWeapon;
                if (
                    CurrWeapon == Weapons.Awp ||
                    CurrWeapon == Weapons.Scar ||
                    CurrWeapon == Weapons.Scout ||
                    CurrWeapon == Weapons.Sig ||
                    CurrWeapon == Weapons.AK47 ||
                    CurrWeapon == Weapons.M4A1 ||
                    CurrWeapon == Weapons.M4A4 ||
                    CurrWeapon == Weapons.SG553 ||
                    CurrWeapon == Weapons.GALIL ||
                    CurrWeapon == Weapons.FAMAS ||
                    CurrWeapon == Weapons.AUG)
                {
                    PrimaryWeapon = ActiveWeapon;
                }
            }
        }

        public bool IsArmingBomb(GameProcess gameProcess)
        {
            int activeWeapon = gameProcess.Process.Read<int>(AddressBase + Offsets.m_hActiveWeapon) & 0xfff;
            IntPtr weaponEntity = gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwEntityList + (activeWeapon - 1) * 0x10);
            short weaponID = gameProcess.Process.Read<short>(weaponEntity + Offsets.m_iItemDefinitionIndex);

            if((Weapons)weaponID == Weapons.C4)
            {
                bool m_bStartedArming = gameProcess.Process.Read<bool>(weaponEntity + Offsets.m_bStartedArming);
                if(m_bStartedArming)
                {
                    return true;
                }
            }
            
            return false;
        }

        public bool IsDefusingBomb(GameProcess gameProcess)
        {
            if (Offsets.m_bIsDefusing == 0) return false;
            return gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bIsDefusing);
        }

        public bool checkIfHasDefuseKit(GameProcess gameProcess)
        {
            if (Offsets.m_bHasDefuser == 0) return false;
            return gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bHasDefuser);
        }

        public bool checkIfAds(GameProcess gameProcess)
        {
            if (Offsets.m_bIsScoped == 0) return false;
            bool isScoping = gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bIsScoped);
            return isScoping;
        }

        public bool checkIfCanShoot(GameProcess gameProcess)
        {
            float m_flNextAttack = gameProcess.Process.Read<float>(AddressBase + Offsets.m_flNextAttack);
            float curtime = Program.GameData.MatchInfo.GlobalVars.curtime;

            if((m_flNextAttack - curtime) > 0)
            {
                return false;
            } else
            {
                return true;
            }
        }
        public bool checkIfCursorLocked(GameProcess gameProcess)
        {
            bool CursorLocked = (gameProcess.ModuleClient.Read<byte>(Offsets.dwMouseEnable) & 1) == 1 ? true : false;
            return CursorLocked;
        }

        public int BulletCounter(GameProcess gameProcess)
        {
            if (Offsets.m_iShotsFired == 0) return 0;
            return gameProcess.Process.Read<int>(AddressBase + Offsets.m_iShotsFired);
        }

        public int AmmoCounter(GameProcess gameProcess)
        {
            int activeWeapon = gameProcess.Process.Read<int>(AddressBase + Offsets.m_hActiveWeapon) & 0xfff;
            IntPtr weaponEntity = gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwEntityList + (activeWeapon - 1) * 0x10);

            if (weaponEntity != IntPtr.Zero)
            {
               return gameProcess.Process.Read<int>(weaponEntity + Offsets.m_iClip1);
            }

            return 0;
        }

        public Vector3 getPlayerVecVelocity(GameProcess gameProcess)
        {
            return gameProcess.Process.Read<Vector3>(AddressBase + Offsets.m_vecVelocity);
        }

        public Vector3 getPlayerEyePosition(GameProcess gameProcess)
        {
            ViewOffset = gameProcess.Process.Read<Vector3>(AddressBase + Offsets.m_vecViewOffset);
            EyePosition = Origin + ViewOffset;
            return EyePosition;
        }

        public override bool Update(GameProcess gameProcess)
        {
            if (!base.Update(gameProcess))
            {
                return false;
            }

            // Read data
            EyePosition = getPlayerEyePosition(gameProcess);
            ViewAngles = gameProcess.Process.Read<Vector3>(gameProcess.ModuleEngine.Read<IntPtr>(Offsets.dwClientState) + Offsets.dwClientState_ViewAngles);
            EyeDirection = GfxMath.GetVectorFromEulerAngles(GfxMath.DegreeToRadian(ViewAngles.X), GfxMath.DegreeToRadian(ViewAngles.Y));
            AimPunchAngle = gameProcess.Process.Read<Vector3>(AddressBase + Offsets.m_aimPunchAngle);
            Fov = gameProcess.Process.Read<int>(AddressBase + Offsets.m_iFOV);
            if (Fov == 0) Fov = 90; // correct for default

            // Update player info
            vecVelocity = getPlayerVecVelocity(gameProcess);
            ActiveWeapon = getActiveWeapon(gameProcess);
            UpdatePrimaryWeapon(ActiveWeapon);
            AmmoCount = AmmoCounter(gameProcess);
            HasDefuseKit = checkIfHasDefuseKit(gameProcess);
            isAimingDownScope = checkIfAds(gameProcess);
            CanShoot = checkIfCanShoot(gameProcess);
            CursorLocked = checkIfCursorLocked(gameProcess);
            Location = getPlayerLocation(gameProcess);

            // calc data
            AimDirection = GetAimDirection(ViewAngles, AimPunchAngle);

            return true;
        }
        private static Vector3 GetAimDirection(Vector3 viewAngles, Vector3 aimPunchAngle)
        {
            var phi = (viewAngles.X + aimPunchAngle.X * weapon_recoil_scale).DegreeToRadian();
            var theta = (viewAngles.Y + aimPunchAngle.Y * weapon_recoil_scale).DegreeToRadian();

            // https://en.wikipedia.org/wiki/Spherical_coordinate_system
            return new Vector3
            (
                (float)(Math.Cos(phi) * Math.Cos(theta)),
                (float)(Math.Cos(phi) * Math.Sin(theta)),
                (float)-Math.Sin(phi)
            ).Normalized();
        }

    }
}

using System;
using System.Runtime.InteropServices;
using ScriptKidAntiCheat.Data;
using ScriptKidAntiCheat.Internal.Raw;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Utils.Maths;
using SharpDX;

/*
 * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
 */
namespace ScriptKidAntiCheat.Internal
{

    public class Entity : EntityBase
    {

        public int Index { get; }

        public bool Dormant { get; private set; } = true;

        private IntPtr AddressStudioHdr { get; set; }

        /// <inheritdoc cref="studiohdr_t"/>
        public studiohdr_t StudioHdr { get; private set; }

        /// <inheritdoc cref="mstudiohitboxset_t"/>
        public mstudiohitboxset_t StudioHitBoxSet { get; private set; }

        /// <inheritdoc cref="mstudiobbox_t"/>
        public mstudiobbox_t[] StudioHitBoxes { get; }

        /// <inheritdoc cref="mstudiobone_t"/>
        public mstudiobone_t[] StudioBones { get; }

        public Matrix[] BonesMatrices { get; }

        public Vector3[] BonesPos { get; }

        public (int from, int to)[] Skeleton { get; }

        public int SkeletonCount { get; private set; }

        public bool Spotted { get; private set; } = false;

        public string Location { get; private set; }

        public string NickName { get; protected set; }

        public Entity(int index)
        {
            Index = index;
            StudioHitBoxes = new mstudiobbox_t[Helper.MaxStudioBones];
            StudioBones = new mstudiobone_t[Helper.MaxStudioBones];
            BonesMatrices = new Matrix[Helper.MaxStudioBones];
            BonesPos = new Vector3[Helper.MaxStudioBones];
            Skeleton = new (int, int)[Helper.MaxStudioBones];
        }

        public override bool IsAlive()
        {
            return base.IsAlive() && !Dormant;
        }

        protected override IntPtr ReadAddressBase(GameProcess gameProcess)
        {
            return gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwEntityList + Index * 0x10 /* size */);
        }

        public override bool Update(GameProcess gameProcess)
        {
            if (!base.Update(gameProcess))
            {
                return false;
            }

            Dormant = gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bDormant);

            if (!IsAlive())
            {
                return true;
            }

            UpdateStudioHdr(gameProcess);
            UpdateStudioHitBoxes(gameProcess);
            UpdateStudioBones(gameProcess);
            UpdateBonesMatricesAndPos(gameProcess);
            UpdateSkeleton();

            Location = MemoryRead.ReadString(gameProcess.ModuleClient, AddressBase, Offsets.m_szLastPlaceName, 18);

            Spotted = gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bSpotted);

            NickName = GetPlayerName(gameProcess);

            return true;
        }

        private void UpdateStudioHdr(GameProcess gameProcess)
        {
            var addressToAddressStudioHdr = gameProcess.Process.Read<IntPtr>(AddressBase + Offsets.m_pStudioHdr);
            AddressStudioHdr = gameProcess.Process.Read<IntPtr>(addressToAddressStudioHdr); // deref
            StudioHdr = gameProcess.Process.Read<studiohdr_t>(AddressStudioHdr);
        }

        private void UpdateStudioHitBoxes(GameProcess gameProcess)
        {
            var addressHitBoxSet = AddressStudioHdr + StudioHdr.hitboxsetindex;
            StudioHitBoxSet = gameProcess.Process.Read<mstudiohitboxset_t>(addressHitBoxSet);

            // read
            for (var i = 0; i < StudioHitBoxSet.numhitboxes; i++)
            {
                StudioHitBoxes[i] = gameProcess.Process.Read<mstudiobbox_t>(addressHitBoxSet + StudioHitBoxSet.hitboxindex + i * Marshal.SizeOf<mstudiobbox_t>());
            }
        }
        private void UpdateStudioBones(GameProcess gameProcess)
        {
            for (var i = 0; i < StudioHdr.numbones; i++)
            {
                StudioBones[i] = gameProcess.Process.Read<mstudiobone_t>(AddressStudioHdr + StudioHdr.boneindex + i * Marshal.SizeOf<mstudiobone_t>());
            }
        }

        private void UpdateBonesMatricesAndPos(GameProcess gameProcess)
        {

            var addressBoneMatrix = gameProcess.Process.Read<IntPtr>(AddressBase + Offsets.m_dwBoneMatrix);
            for (var boneId = 0; boneId < BonesPos.Length; boneId++)
            {
                var matrix = gameProcess.Process.Read<matrix3x4_t>(addressBoneMatrix + boneId * Marshal.SizeOf<matrix3x4_t>());
                BonesMatrices[boneId] = matrix.ToMatrix();
                BonesPos[boneId] = new Vector3(matrix.m30, matrix.m31, matrix.m32);
            }
        }
        private void UpdateSkeleton()
        {
            // get bones to draw
            var skeletonBoneId = 0;
            for (var i = 0; i < StudioHitBoxSet.numhitboxes; i++)
            {
                var hitbox = StudioHitBoxes[i];
                var bone = StudioBones[hitbox.bone];
                if (bone.parent >= 0 && bone.parent < StudioHdr.numbones)
                {
                    // has valid parent
                    Skeleton[skeletonBoneId] = (hitbox.bone, bone.parent);
                    skeletonBoneId++;
                }
            }
            SkeletonCount = skeletonBoneId;
        }
        private string GetPlayerName(GameProcess gameProcess)
        {
            var radarBase = gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwRadarBase);

            if (radarBase == IntPtr.Zero) return string.Empty;

            var radarPtr = gameProcess.Process.Read<IntPtr>(radarBase + Offsets.m_iradarBasePtr);

            if (radarPtr == IntPtr.Zero) return string.Empty;

            var nameAddr = radarPtr + Index * Offsets.m_iradarStructSize + Offsets.m_iradarStructPos;

            string NickName = MemoryRead.ReadString(gameProcess.ModuleClient, nameAddr, 0, 64);

            return NickName;
        }

    }
}

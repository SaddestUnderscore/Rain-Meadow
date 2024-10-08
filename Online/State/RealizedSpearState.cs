using System;
using UnityEngine;

namespace RainMeadow
{
    public class RealizedSpearState : RealizedWeaponState
    {
        [OnlineField(group = "spear", nullable = true)]
        private Vector2? stuckInWall;
        [OnlineField(group = "spear", nullable = true)]
        private OnlineEntity.EntityId stuckInObject;
        [OnlineField(group = "spear", nullable = true)]
        private AppendageRef stuckInAppendage;
        [OnlineField(group = "spear")]
        private byte stuckInChunkIndex;
        [OnlineField(group = "spear")]
        private sbyte stuckBodyPart;
        [OnlineField(group = "spear")]
        private float stuckRotation;
        [OnlineField(group = "spear")]
        private int stuckInWallCycles;

        public RealizedSpearState() { }
        public RealizedSpearState(OnlinePhysicalObject onlineEntity) : base(onlineEntity)
        {
            var spear = (Spear)onlineEntity.apo.realizedObject;
            stuckInWall = spear.stuckInWall;
            stuckInWallCycles = spear.abstractSpear.stuckInWallCycles;

            if (spear.stuckInObject != null)
            {
                if (!OnlinePhysicalObject.map.TryGetValue(spear.stuckInObject.abstractPhysicalObject, out var onlineStuckEntity))

                    if (RainMeadow.isArenaMode(out var _))
                    {

                        RainMeadow.Debug("Stuck in creature while switching worlds");

                    }

                    else
                    {
                        throw new InvalidOperationException("Stuck to a non-synced creature!");
                    }
                stuckInObject = onlineStuckEntity?.id;
                stuckInChunkIndex = (byte)spear.stuckInChunkIndex;
                stuckInAppendage = spear.stuckInAppendage != null ? new AppendageRef(spear.stuckInAppendage) : null;
                stuckBodyPart = (sbyte)spear.stuckBodyPart;
                stuckRotation = spear.stuckRotation;
            }
        }

        public override void ReadTo(OnlineEntity onlineEntity)
        {
            if (!onlineEntity.owner.isMe && onlineEntity.isPending) return; // Don't sync if pending, reduces visibility and effect of lag
            var spear = (Spear)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;
            spear.stuckInWall = stuckInWall;
            spear.abstractSpear.stuckInWallCycles = stuckInWallCycles;
            if (!stuckInWall.HasValue)
                spear.addPoles = false;

            var stuckInEntity = stuckInObject?.FindEntity() as OnlinePhysicalObject;
            if (stuckInEntity != null)
            {
                spear.stuckInObject = stuckInEntity.apo.realizedObject;
                spear.stuckInAppendage = stuckInAppendage?.GetAppendagePos(stuckInEntity);
            }
            spear.stuckInChunkIndex = stuckInChunkIndex;
            spear.stuckBodyPart = stuckBodyPart;
            spear.stuckRotation = stuckRotation;

            base.ReadTo(onlineEntity);
            if (spear.mode == Weapon.Mode.StuckInWall && !spear.stuckInWall.HasValue)
            {
                RainMeadow.Error("Stuck in wall but has no value!");
                spear.ChangeMode(Weapon.Mode.Free);
            }
            if (spear.mode == Weapon.Mode.StuckInCreature && (stuckInEntity == null || stuckInEntity.apo.realizedObject == null ))
            {
                RainMeadow.Error("Stuck in creature but no creature");
                spear.ChangeMode(Weapon.Mode.Free);
            }
        }
    }

    public class AppendageRef : Serializer.ICustomSerializable, IEquatable<AppendageRef>
    {
        public byte appIndex;
        public byte prevSegment;
        public float distanceToNext;

        public AppendageRef() { }
        public AppendageRef(PhysicalObject.Appendage.Pos appendagePos)
        {
            appIndex = (byte)appendagePos.appendage.appIndex;
            prevSegment = (byte)appendagePos.prevSegment;
            distanceToNext = appendagePos.distanceToNext;
        }

        public void CustomSerialize(Serializer serializer)
        {
            serializer.Serialize(ref appIndex);
            serializer.Serialize(ref prevSegment);
            serializer.SerializeHalf(ref distanceToNext);
        }

        public bool Equals(AppendageRef other)
        {
            return other != null && other.appIndex == appIndex && other.prevSegment == prevSegment && other.distanceToNext == distanceToNext;
        }

        public override bool Equals(object obj) => Equals(obj as AppendageRef);

        public override int GetHashCode() => appIndex + prevSegment + (int)(1024 * distanceToNext);

        public PhysicalObject.Appendage.Pos GetAppendagePos(OnlinePhysicalObject appendageOwner)
        {
            if (appendageOwner == null) return null;
            var physicalObject = appendageOwner.apo.realizedObject;
            var appendage = physicalObject.appendages[appIndex];
            return new PhysicalObject.Appendage.Pos(appendage, prevSegment, distanceToNext);
        }
    }
}

using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public readonly struct TankGroundInfo
    {
        public static TankGroundInfo NoGround =>
            new TankGroundInfo(
                false,
                Vector3.zero,
                Vector3.up,
                float.PositiveInfinity);

        public bool HasGround { get; }
        public Vector3 GroundPoint { get; }
        public Vector3 GroundNormal { get; }
        public float Distance { get; }

        public TankGroundInfo(
            bool hasGround,
            Vector3 groundPoint,
            Vector3 groundNormal,
            float distance)
        {
            HasGround = hasGround;
            GroundPoint = groundPoint;
            GroundNormal = groundNormal;
            Distance = distance;
        }
    }
}
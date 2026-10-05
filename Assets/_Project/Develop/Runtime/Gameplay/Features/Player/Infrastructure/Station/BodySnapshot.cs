using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Station
{
    internal readonly struct PlayerStationBodySnapshot
    {
        public Transform Parent { get; }
        public Vector3 LocalScale { get; }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public Vector3 LinearVelocity { get; }
        public Vector3 AngularVelocity { get; }

        public bool IsKinematic { get; }
        public bool UseGravity { get; }
        public bool DetectCollisions { get; }
        public bool ColliderEnabled { get; }
        public RigidbodyInterpolation Interpolation { get; }

        public PlayerStationBodySnapshot(
            Transform parent,
            Vector3 localScale,
            Vector3 position,
            Quaternion rotation,
            Vector3 linearVelocity,
            Vector3 angularVelocity,
            bool isKinematic,
            bool useGravity,
            bool detectCollisions,
            bool colliderEnabled,
            RigidbodyInterpolation interpolation)
        {
            Parent = parent;
            LocalScale = localScale;

            Position = position;
            Rotation = rotation;

            LinearVelocity = linearVelocity;
            AngularVelocity = angularVelocity;

            IsKinematic = isKinematic;
            UseGravity = useGravity;
            DetectCollisions = detectCollisions;
            ColliderEnabled = colliderEnabled;
            Interpolation = interpolation;
        }
    }
}
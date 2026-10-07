using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Ammunition;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application
{
    public readonly struct GunShotRequest
    {
        public ShellDefinition Shell { get; }
        public Pose MuzzlePose { get; }
        public Vector3 InheritedTankVelocity { get; }
        public ulong ActorId { get; }
        public ulong TankId { get; }

        public GunShotRequest(
            ShellDefinition shell,
            Pose muzzlePose,
            Vector3 inheritedTankVelocity,
            ulong actorId,
            ulong tankId)
        {
            Shell = shell;
            MuzzlePose = muzzlePose;
            InheritedTankVelocity = inheritedTankVelocity;
            ActorId = actorId;
            TankId = tankId;
        }
    }
}
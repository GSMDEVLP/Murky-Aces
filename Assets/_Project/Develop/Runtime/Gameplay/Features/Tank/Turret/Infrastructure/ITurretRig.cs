using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public interface ITurretRig
    {
        float HullYaw { get; }
        Pose MuzzlePose { get; }
        Transform BreechLoadPoint { get; }

        void ApplyAngles(float yaw, float pitch);
    }
}
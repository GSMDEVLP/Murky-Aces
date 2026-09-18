using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public interface ITankMotionBody
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }

        void ResolveAndApplyMotion(
            Vector3 desiredDisplacement,
            Quaternion desiredRotation);
    }
}
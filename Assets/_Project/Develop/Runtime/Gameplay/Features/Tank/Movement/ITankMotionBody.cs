using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public interface ITankMotionBody
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        Vector3 LinearVelocity { get; }
        Vector3 AngularVelocity { get; }

        void ApplyLinearAcceleration(Vector3 acceleration);
        void ApplyAngularAcceleration(Vector3 acceleration);
    }
}
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions
{
    public interface IDroneFlightBody
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }

        void Initialize();
        void ResetDistanceSample();
        float MeasureTravelledDistance();
        void ApplyFlight(float speed, float headingOffsetDegrees);
        void ApplyVelocity(Vector3 velocity);
        void Stop();
    }
}

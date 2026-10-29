namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions
{
    public interface IDroneFlightBody
    {
        void Initialize();
        void ResetDistanceSample();
        float MeasureTravelledDistance();
        void ApplyFlight(float speed, float headingOffsetDegrees);
        void Stop();
    }
}

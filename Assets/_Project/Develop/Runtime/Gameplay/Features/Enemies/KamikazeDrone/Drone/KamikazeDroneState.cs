namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Domain
{

    public sealed class KamikazeDroneState
    {
        public KamikazeDroneMode Mode { get; internal set; }
        public float Speed { get; internal set; }
        public float DistanceTravelled { get; internal set; }
        public float PauseRemaining { get; internal set; }

        // Угол относительно начального направления экземпляра.
        public float HeadingOffsetDegrees { get; internal set; }
    }
}
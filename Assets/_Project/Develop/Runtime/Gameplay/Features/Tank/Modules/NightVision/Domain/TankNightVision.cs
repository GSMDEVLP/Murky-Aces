namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain
{
    public sealed class TankNightVision
    {
        public bool IsEnabled { get; private set; }

        public void SetEnabled(bool enabled)
        {
            if (IsEnabled == enabled)
                return;

            IsEnabled = enabled;
        }
    }
}
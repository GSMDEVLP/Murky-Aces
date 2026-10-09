namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Domain
{
    public sealed class TankLights
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
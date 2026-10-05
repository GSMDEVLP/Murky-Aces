namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret
{
    public sealed class GunnerZoomState
    {
        public bool IsHeld { get; private set; }

        public void SetHeld(bool held) => IsHeld = held;
        public void Clear() => IsHeld = false;
    }
}
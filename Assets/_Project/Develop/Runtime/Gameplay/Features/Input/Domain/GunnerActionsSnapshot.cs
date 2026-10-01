namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct GunnerActionsSnapshot
    {
        public static GunnerActionsSnapshot Neutral => new GunnerActionsSnapshot(
                0f, 0f,
                false, false, false,
                false, false,
                false, false);

        public float AimX { get; }
        public float AimY { get; }

        public bool FirePressedThisFrame { get; }
        public bool FireHeld { get; }
        public bool ZoomHeld { get; }

        public bool TogglePressedThisFrame { get; }
        public bool ToggleHeld { get; }

        public bool ExitPressedThisFrame { get; }
        public bool ExitHeld { get; }

        public GunnerActionsSnapshot(
            float aimX,
            float aimY,
            bool firePressedThisFrame,
            bool fireHeld,
            bool zoomHeld,
            bool togglePressedThisFrame,
            bool toggleHeld,
            bool exitPressedThisFrame,
            bool exitHeld)
        {
            AimX = aimX;
            AimY = aimY;
            FirePressedThisFrame = firePressedThisFrame;
            FireHeld = fireHeld;
            ZoomHeld = zoomHeld;
            TogglePressedThisFrame = togglePressedThisFrame;
            ToggleHeld = toggleHeld;
            ExitPressedThisFrame = exitPressedThisFrame;
            ExitHeld = exitHeld;
        }
    }
}
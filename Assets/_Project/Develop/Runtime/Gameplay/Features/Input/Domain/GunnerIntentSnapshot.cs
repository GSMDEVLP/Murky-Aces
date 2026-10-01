namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct GunnerIntentSnapshot
    {
        public static GunnerIntentSnapshot Neutral => new GunnerIntentSnapshot(0f, 0f, false, false, false);

        public float Traverse { get; }
        public float Elevation { get; }
        public bool ZoomHeld { get; }
        public bool FireRequested { get; }
        public bool ToggleReferenceModeRequested { get; }

        public GunnerIntentSnapshot(
            float traverse,
            float elevation,
            bool zoomHeld,
            bool fireRequested,
            bool toggleReferenceModeRequested)
        {
            Traverse = ClampAxis(traverse);
            Elevation = ClampAxis(elevation);
            ZoomHeld = zoomHeld;
            FireRequested = fireRequested;
            ToggleReferenceModeRequested =
                toggleReferenceModeRequested;
        }

        private static float ClampAxis(float value)
        {
            if (value < -1f) return -1f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
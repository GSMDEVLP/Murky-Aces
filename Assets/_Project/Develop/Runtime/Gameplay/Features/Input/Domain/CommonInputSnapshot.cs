namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct CommonInputSnapshot
    {
        public static CommonInputSnapshot Neutral =>
            new CommonInputSnapshot(
                0f,
                0f,
                false,
                false,
                false,
                false,
                false);

        public float LookX { get; }
        public float LookY { get; }
        public bool LookIsPointerDelta { get; }

        public bool InteractPressedThisFrame { get; }
        public bool InteractHeld { get; }
        public bool InteractReleasedThisFrame { get; }

        public bool DropPressedThisFrame { get; }

        public CommonInputSnapshot(
            float lookX,
            float lookY,
            bool lookIsPointerDelta,
            bool interactPressedThisFrame,
            bool interactHeld,
            bool interactReleasedThisFrame,
            bool dropPressedThisFrame)
        {
            LookX = lookX;
            LookY = lookY;
            LookIsPointerDelta = lookIsPointerDelta;
            InteractPressedThisFrame = interactPressedThisFrame;
            InteractHeld = interactHeld;
            InteractReleasedThisFrame = interactReleasedThisFrame;
            DropPressedThisFrame = dropPressedThisFrame;
        }
    }
}
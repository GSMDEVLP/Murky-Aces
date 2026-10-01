namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct PlayerActionsSnapshot
    {
        public static PlayerActionsSnapshot Neutral =>
            new PlayerActionsSnapshot(
                0f,
                0f,
                false,
                false);

        public float MoveX { get; }
        public float MoveY { get; }
        public bool SprintHeld { get; }
        public bool JumpHeld { get; }

        public PlayerActionsSnapshot(
            float moveX,
            float moveY,
            bool sprintHeld,
            bool jumpHeld)
        {
            MoveX = moveX;
            MoveY = moveY;
            SprintHeld = sprintHeld;
            JumpHeld = jumpHeld;
        }
    }
}
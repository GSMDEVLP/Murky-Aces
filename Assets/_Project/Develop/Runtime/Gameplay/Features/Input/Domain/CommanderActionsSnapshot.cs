namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct CommanderActionsSnapshot
    {
        public static CommanderActionsSnapshot Neutral => new CommanderActionsSnapshot(false, false);
        public bool ExitPressedThisFrame { get; }
        public bool ExitHeld { get; }

        public CommanderActionsSnapshot(bool exitPressedThisFrame, bool exitHeld)
        {
            ExitPressedThisFrame = exitPressedThisFrame;
            ExitHeld = exitHeld;
        }
    }
}
namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public readonly struct InteractionActionState
    {
        public InteractionPromts Prompt { get; }
        public bool IsAvailable { get; }

        public InteractionActionState(
            InteractionPromts prompt,
            bool isAvailable)
        {
            Prompt = prompt;
            IsAvailable = isAvailable;
        }
    }
}
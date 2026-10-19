using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions
{
    public interface IPlayerGpsSession
    {
        bool IsActive { get; }
        bool IsReady { get; }

        bool CanBegin(
            IGpsScreen screen,
            IInteractionActor actor);

        bool TryBegin(
            IGpsScreen screen,
            IInteractionActor actor);

        bool TryForceClose();
    }
}
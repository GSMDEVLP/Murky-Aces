using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions
{
    public interface IPlayerStationCapabilities
    {
        bool AreDisabledForStation { get; }

        bool TryApplyForStation(
            IStationCapabilityProfile profile,
            IInteractionScope interactionScope);

        bool TryRestore();

        bool TryRestoreBeforeStation();
    }
}
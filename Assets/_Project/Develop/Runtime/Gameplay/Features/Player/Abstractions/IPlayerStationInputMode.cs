using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions
{
    public interface IPlayerStationInputMode
    {
        StationControlContext Context { get; }

        bool IsActive { get; }

        bool TryActivate();

        bool TryDeactivate();

        bool TryRestoreAfterFailedExit();

        bool ConsumeExitRequest();
    }
}
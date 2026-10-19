using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions
{
    public interface IPlayerInputContext
    {
        InputMapId CurrentMap { get; }
        bool IsUsingPlayerMap { get; }
        bool IsUsingDrivingMap { get; }
        bool IsUsingGunnerMap { get; }
        bool TryUseMap(InputMapId map);
        bool TryUsePlayerMap();
        bool TryUseDrivingMap();
        bool TryUseGunnerMap();
    }
}
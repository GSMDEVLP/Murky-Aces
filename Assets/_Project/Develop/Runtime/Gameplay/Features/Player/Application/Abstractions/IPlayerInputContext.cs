namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IPlayerInputContext
    {
        bool IsUsingPlayerMap { get; }
        bool IsUsingDrivingMap { get; }

        bool TryUsePlayerMap();
        bool TryUseDrivingMap();
    }
}
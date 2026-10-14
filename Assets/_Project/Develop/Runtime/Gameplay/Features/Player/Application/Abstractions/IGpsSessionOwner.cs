using _Project.Develop.Runtime.Gameplay.Features.Player.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IGpsSessionOwner
    {
        PlayerGpsSession GpsSession { get; }
    }
}
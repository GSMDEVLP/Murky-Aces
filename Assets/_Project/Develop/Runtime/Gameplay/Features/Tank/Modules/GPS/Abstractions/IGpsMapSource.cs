using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions
{
    public interface IGpsMapSource
    {
        bool TryGetMap(out GpsMapData map);
    }
}
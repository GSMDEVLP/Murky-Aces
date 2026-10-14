using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions
{
    public interface IGpsRouteReader
    {
        IReadOnlyList<GpsRoutePoint> Points { get; }
        GpsRoutePoint? Destination { get; }
        long Revision { get; }
    }
}
using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application
{
    public sealed class GpsRouteService
    {
        private readonly GpsRouteState _route;
        private readonly CrewStationRegistry _stations;

        public GpsRouteService(GpsRouteState route, CrewStationRegistry stations)
        {
            _route = route ??
                throw new ArgumentNullException(nameof(route));

            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));
        }

        public bool CanEdit(IInteractionActor actor)
        {
            return actor != null &&
                   _stations.TryGet(
                       CrewRoleId.CommanderLoader,
                       out CrewStationController station) &&
                   station.IsOccupiedBy(actor.Id);
        }

        public bool TryAppendPoint(IInteractionActor actor, GpsRoutePoint point)
        {
            if (!CanEdit(actor))
                return false;

            _route.AppendPoint(point);
            return true;
        }

        public bool TryRemoveLastPoint(IInteractionActor actor)
        {
            if (!CanEdit(actor))
                return false;

            return _route.TryRemoveLastPoint();
        }
        public bool TrySetDestination(IInteractionActor actor, GpsRoutePoint destination)
        {
            if (!CanEdit(actor))
                return false;

            _route.SetDestination(destination);
            return true;
        }

        public bool TryClear(IInteractionActor actor)
        {
            if (!CanEdit(actor))
                return false;

            return _route.TryClear();
        }
    }
}
using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application
{
    public sealed class GpsRouteService
    {
        private readonly GpsRouteState _route;
        private readonly CrewStationRegistry _stations;
        private readonly ITankMotionBody _body;
        private readonly IGpsMapSource _mapSource;
        private readonly GpsPolylineRoutePlanner _planner;

        public GpsRouteService(
            GpsRouteState route,
            CrewStationRegistry stations,
            ITankMotionBody body,
            IGpsMapSource mapSource,
            GpsPolylineRoutePlanner planner)
        {
            _route = route ??
                throw new ArgumentNullException(nameof(route));

            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));

            _body = body ??
                throw new ArgumentNullException(nameof(body));

            _mapSource = mapSource ??
                throw new ArgumentNullException(nameof(mapSource));

            _planner = planner ??
                throw new ArgumentNullException(nameof(planner));
        }

        public bool CanEdit(IInteractionActor actor)
        {
            return actor != null &&
                   _stations.TryGet(
                       CrewRoleId.CommanderLoader,
                       out CrewStationController station) &&
                   station.IsOccupiedBy(actor.Id);
        }

        public bool TrySetDestination(
            IInteractionActor actor,
            GpsRoutePoint destination)
        {
            if (!CanEdit(actor))
                return false;

            if (!_mapSource.TryGetMap(out GpsMapData map) ||
                map == null)
            {
                return false;
            }

            Vector3 position = _body.Position;

            var start = new GpsRoutePoint(
                new Vector2(position.x, position.z));

            if (!_planner.TryBuild(
                    map.RoadPoints,
                    start,
                    destination,
                    out var points))
            {
                return false;
            }

            _route.SetRoute(points);
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
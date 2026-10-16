using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application
{
    public sealed class GpsRouteService
    {
        private readonly GpsRouteState _route;
        private readonly CrewStationRegistry _stations;
        private readonly ITankMotionBody _body;

        public GpsRouteService(
            GpsRouteState route,
            CrewStationRegistry stations,
            ITankMotionBody body)
        {
            _route = route ??
                throw new ArgumentNullException(nameof(route));

            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));

            _body = body ??
                throw new ArgumentNullException(nameof(body));
        }

        public bool CanEdit(IInteractionActor actor)
        {
            return actor != null &&
                   _stations.TryGet(
                       CrewRoleId.CommanderLoader,
                       out CrewStationController station) &&
                   station.IsOccupiedBy(actor.Id);
        }

        public bool TrySetDestination(IInteractionActor actor, GpsRoutePoint destination)
        {
            if (!CanEdit(actor))
                return false;

            SetDirectRoute(ReadCurrentPosition(), destination);
            return true;
        }

        public bool TryClear(IInteractionActor actor)
        {
            if (!CanEdit(actor))
                return false;

            return _route.TryClear();
        }

        internal bool TryRefreshRoute(float minimumMovement)
        {
            GpsRoutePoint? destination = _route.Destination;

            if (!destination.HasValue || _route.Points.Count == 0)
                return false;

            GpsRoutePoint start = ReadCurrentPosition();

            Vector2 offset = start.WorldPosition -
                            _route.Points[0].WorldPosition;

            if (offset.sqrMagnitude < minimumMovement * minimumMovement)
                return false;

            SetDirectRoute(start, destination.Value);
            return true;
        }
        
        internal bool TryCompleteRoute(float arrivalRadius)
        {
            GpsRoutePoint? destination = _route.Destination;

            if (!destination.HasValue)
                return false;

            GpsRoutePoint current = ReadCurrentPosition();

            Vector2 offset = current.WorldPosition -
                            destination.Value.WorldPosition;

            if (offset.sqrMagnitude > arrivalRadius * arrivalRadius)
                return false;

            return _route.TryClear();
        }

        private GpsRoutePoint ReadCurrentPosition()
        {
            Vector3 position = _body.Position;

            return new GpsRoutePoint(
                new Vector2(position.x, position.z));
        }

        private void SetDirectRoute(
            GpsRoutePoint start,
            GpsRoutePoint destination)
        {
            var points = new[]
            {
                start,
                destination
            };

            _route.SetRoute(points, destination);
        }
    }
}
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain
{
    public sealed class GpsRouteState : IGpsRouteReader
    {
        private readonly List<GpsRoutePoint> _points = new();
        private readonly IReadOnlyList<GpsRoutePoint> _readOnlyPoints;

        public IReadOnlyList<GpsRoutePoint> Points => _readOnlyPoints;
        public GpsRoutePoint? Destination { get; private set; }
        public long Revision { get; private set; }

        public GpsRouteState()
        {
            _readOnlyPoints = _points.AsReadOnly();
        }

        internal void SetRoute(IReadOnlyList<GpsRoutePoint> points, GpsRoutePoint destination)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (points.Count == 0)
            {
                throw new ArgumentException(
                    "GPS route must contain at least one point.",
                    nameof(points));
            }

            var copy = new GpsRoutePoint[points.Count];

            for (int i = 0; i < points.Count; i++)
                copy[i] = points[i];

            _points.Clear();
            _points.AddRange(copy);

            Destination = destination;
            Revision++;
        }

        internal bool TryClear()
        {
            if (_points.Count == 0 && !Destination.HasValue)
                return false;

            _points.Clear();
            Destination = null;
            Revision++;

            return true;
        }
    }
}
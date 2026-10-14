using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain
{
    public sealed class GpsRouteState : IGpsRouteReader
    {
        private readonly List<GpsRoutePoint> _points = new();
        private readonly IReadOnlyList<GpsRoutePoint> _readOnlyPoints;
        public GpsRoutePoint? Destination { get; private set; }
        public IReadOnlyList<GpsRoutePoint> Points => _readOnlyPoints;
        public long Revision { get; private set; }

        public GpsRouteState()
        {
            _readOnlyPoints = _points.AsReadOnly();
        }

        internal void SetDestination(GpsRoutePoint destination)
        {
            Destination = destination;
            _points.Clear();
            Revision++;
        }

        internal void AppendPoint(GpsRoutePoint point)
        {
            _points.Add(point);
            Revision++;
        }

        internal bool TryRemoveLastPoint()
        {
            if (_points.Count == 0)
                return false;

            _points.RemoveAt(_points.Count - 1);
            Revision++;

            return true;
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
namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class DriverStation
    {
        public DriverStationState State { get; private set; } = DriverStationState.Free;

        public ulong? OccupantId { get; private set; }

        public bool IsFree => State == DriverStationState.Free;

        public bool IsOccupied => State == DriverStationState.Occupied;

        public bool IsOccupiedBy(ulong occupantId)
        {
            return State == DriverStationState.Occupied &&
                   OccupantId == occupantId;
        }

        public bool TryBeginEnter(ulong occupantId)
        {
            if (State != DriverStationState.Free || OccupantId.HasValue)
            {
                return false;
            }

            State = DriverStationState.Entering;
            OccupantId = occupantId;
            return true;
        }

        public bool TryCompleteEnter(ulong occupantId)
        {
            return TryTransition(
                DriverStationState.Entering,
                DriverStationState.Occupied,
                occupantId);
        }

        public bool TryCancelEnter(ulong occupantId)
        {
            return TryTransition(
                DriverStationState.Entering,
                DriverStationState.Free,
                occupantId,
                releaseOccupant: true);
        }

        public bool TryBeginExit(ulong occupantId)
        {
            return TryTransition(
                DriverStationState.Occupied,
                DriverStationState.Exiting,
                occupantId);
        }

        public bool TryCompleteExit(ulong occupantId)
        {
            return TryTransition(
                DriverStationState.Exiting,
                DriverStationState.Free,
                occupantId,
                releaseOccupant: true);
        }

        public bool TryCancelExit(ulong occupantId)
        {
            return TryTransition(
                DriverStationState.Exiting,
                DriverStationState.Occupied,
                occupantId);
        }

        private bool TryTransition(DriverStationState expected, DriverStationState next, ulong occupantId, bool releaseOccupant = false)
        {
            if (State != expected ||OccupantId != occupantId)
            {
                return false;
            }

            State = next;

            if (releaseOccupant)
                OccupantId = null;

            return true;
        }
    }
}
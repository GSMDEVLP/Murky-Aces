namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationOccupancy
    {
        public CrewStationState State { get; private set; } = CrewStationState.Free;

        public ulong? OccupantId { get; private set; }

        public bool IsFree => State == CrewStationState.Free;

        public bool IsOccupied => State == CrewStationState.Occupied;

        public bool IsOccupiedBy(ulong occupantId)
        {
            return State == CrewStationState.Occupied &&
                   OccupantId == occupantId;
        }

        public bool TryBeginEnter(ulong occupantId)
        {
            if (State != CrewStationState.Free ||
                OccupantId.HasValue)
            {
                return false;
            }

            State = CrewStationState.Entering;
            OccupantId = occupantId;

            return true;
        }

        public bool TryCompleteEnter(ulong occupantId)
        {
            return TryTransition(
                CrewStationState.Entering,
                CrewStationState.Occupied,
                occupantId);
        }

        public bool TryCancelEnter(ulong occupantId)
        {
            return TryTransition(
                CrewStationState.Entering,
                CrewStationState.Free,
                occupantId,
                releaseOccupant: true);
        }

        public bool TryBeginExit(ulong occupantId)
        {
            return TryTransition(
                CrewStationState.Occupied,
                CrewStationState.Exiting,
                occupantId);
        }

        public bool TryCompleteExit(ulong occupantId)
        {
            return TryTransition(
                CrewStationState.Exiting,
                CrewStationState.Free,
                occupantId,
                releaseOccupant: true);
        }

        public bool TryCancelExit(ulong occupantId)
        {
            return TryTransition(
                CrewStationState.Exiting,
                CrewStationState.Occupied,
                occupantId);
        }

        private bool TryTransition(
            CrewStationState expectedState,
            CrewStationState nextState,
            ulong occupantId,
            bool releaseOccupant = false)
        {
            if (State != expectedState || OccupantId != occupantId)
            {
                return false;
            }

            State = nextState;

            if (releaseOccupant)
                OccupantId = null;

            return true;
        }
    }
}
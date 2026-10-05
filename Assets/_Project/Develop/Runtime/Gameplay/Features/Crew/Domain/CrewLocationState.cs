namespace _Project.Develop.Runtime.Gameplay.Features.Crew.Domain
{
    public sealed class CrewLocationState
    {
        public CrewLocation Current { get; private set; } =
            CrewLocation.Outside;

        public long Revision { get; private set; }

        public bool CanTransitionTo(CrewLocation next)
        {
            if (next == null)
                return false;

            switch (Current.Kind)
            {
                case CrewLocationKind.Outside:
                    return next.Kind == CrewLocationKind.Interior ||
                           next.Kind == CrewLocationKind.Station;

                case CrewLocationKind.Interior:
                    return next.Kind == CrewLocationKind.Outside ||
                           next.Kind == CrewLocationKind.Station;

                case CrewLocationKind.Station:
                    return next.Kind == CrewLocationKind.Outside ||
                           next.Kind == CrewLocationKind.Interior;

                default:
                    return false;
            }
        }

        internal bool TryCommit(long expectedRevision, CrewLocation next)
        {
            if (Revision != expectedRevision ||
                !CanTransitionTo(next))
            {
                return false;
            }

            Current = next;
            Revision++;

            return true;
        }
    }
}
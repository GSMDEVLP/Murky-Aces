namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public readonly struct InteractionActionOption
    {
        public InteractionActionId Id { get; }
        public InteractionInfo Info { get; }
        public long ActionRevision { get; }
        public InteractionActionOption(InteractionActionId id, InteractionInfo info, long actionRevision = 0)
        {
            Id = id;
            Info = info;
            ActionRevision = actionRevision;
        }
    }
}

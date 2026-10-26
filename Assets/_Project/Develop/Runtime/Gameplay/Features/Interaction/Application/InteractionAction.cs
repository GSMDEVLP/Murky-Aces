using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class InteractionAction : IInteractionAction
    {
        private readonly Func<InteractionContext, InteractionInfo> _getInfo;
        private readonly Func<InteractionContext, bool> _begin;
        private readonly Action<InteractionContext> _complete;
        private readonly Action<InteractionContext, InteractionCancelReason> _cancel;
        private readonly Func<long> _getRevision;
        public InteractionActionId Id { get; }
        public long Revision => _getRevision?.Invoke() ?? 0;
        public InteractionAction(InteractionActionId id,
            Func<InteractionContext, InteractionInfo> getInfo,
            Func<InteractionContext, bool> begin,
            Action<InteractionContext> complete,
            Action<InteractionContext, InteractionCancelReason> cancel,
            Func<long> getRevision = null)
        {
            Id = id;
            _getInfo = getInfo ?? throw new ArgumentNullException(nameof(getInfo));
            _begin = begin ?? throw new ArgumentNullException(nameof(begin));
            _complete = complete ?? throw new ArgumentNullException(nameof(complete));
            _cancel = cancel ?? throw new ArgumentNullException(nameof(cancel));
            _getRevision = getRevision;
        }
        public InteractionInfo GetInteractionInfo(in InteractionContext context) => _getInfo(context);
        public bool Begin(in InteractionContext context) => _begin(context);
        public void Complete(in InteractionContext context) => _complete(context);
        public void Cancel(in InteractionContext context, InteractionCancelReason reason) => _cancel(context, reason);
    }
}

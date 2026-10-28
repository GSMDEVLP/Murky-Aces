using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class InteractionControl
    {
        public bool IsGameplayEnabled { get; private set; } = true;
        public IInteractionScope TargetScope { get; private set; }
        public long Revision { get; private set; }

        public event Action Changed;

        public void SetGameplayEnabled(bool isEnabled)
        {
            if (IsGameplayEnabled == isEnabled)
                return;

            IsGameplayEnabled = isEnabled;
            NotifyChanged();
        }

        public bool TryRestrictTargetScope(IInteractionScope scope)
        {
            if (scope == null)
                return false;

            if (ReferenceEquals(TargetScope, scope))
                return true;

            TargetScope = scope;
            NotifyChanged();
            return true;
        }

        public void ClearTargetScope()
        {
            if (TargetScope == null)
                return;

            TargetScope = null;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            Revision++;
            Changed?.Invoke();
        }
    }
}

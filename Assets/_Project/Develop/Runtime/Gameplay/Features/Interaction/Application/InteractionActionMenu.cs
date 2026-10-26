using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class InteractionActionMenu
    {
        private readonly IInteractionAction[] _actions;
        private readonly InteractionActionOption[] _options;
        private readonly long _targetRevision;
        private readonly InteractionMenuState _state;
        public IInteractionTarget Target { get; }
        public IReadOnlyList<InteractionActionOption> Options { get; }
        public int SelectedIndex => _state.SelectedIndex;
        public IInteractionAction SelectedAction => _actions[SelectedIndex];
        public InteractionActionMenu(IInteractionTarget target,
            List<IInteractionAction> actions, in InteractionContext context)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            _targetRevision = target.Revision;
            _state = new InteractionMenuState(actions.Count);
            _actions = actions.ToArray();
            _options = new InteractionActionOption[_actions.Length];
            for (int i = 0; i < _actions.Length; i++)
                _options[i] = new InteractionActionOption(_actions[i].Id,
                    _actions[i].GetInteractionInfo(context), _actions[i].Revision);
            Options = Array.AsReadOnly(_options);
        }
        public void Move(int direction) => _state.Move(direction);
        public bool CanContinue(IInteractionTarget focusedTarget,
            List<IInteractionAction> availableActions, in InteractionContext context)
        {
            if (!ReferenceEquals(focusedTarget, Target) || !Target.IsActive ||
                Target.Revision != _targetRevision || availableActions.Count != _actions.Length)
                return false;
            for (int i = 0; i < _actions.Length; i++)
                if (!ReferenceEquals(availableActions[i], _actions[i]) ||
                    _actions[i].Revision != _options[i].ActionRevision ||
                    !_options[i].Info.Matches(_actions[i].GetInteractionInfo(context)))
                    return false;
            return true;
        }
    }
}

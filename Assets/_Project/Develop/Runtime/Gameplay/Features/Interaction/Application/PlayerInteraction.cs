using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class PlayerInteraction : ITickable
    {
        private readonly IInteractionInput _input;
        private readonly IInteractionTargetFinder _targetFinder;
        private readonly InteractionContext _context;
        private readonly List<IInteractionAction> _focusedActions = new List<IInteractionAction>(4);
        private IInteractionScope _targetScope;
        private HoldInteractionSession _activeHold;
        private InteractionActionMenu _menu;
        private bool _awaitingMenuRelease;
        private long _menuInputRevision;
        private bool _gameplayEnabled = true;

        public bool HasFocusedTarget { get; private set; }
        public IInteractionTarget FocusedTarget { get; private set; }
        public InteractionInfo FocusedInfo { get; private set; }
        public bool IsGameplayEnabled => _gameplayEnabled;
        public bool IsHolding => _activeHold != null;
        public float ActiveHoldElapsedSeconds => _activeHold?.ElapsedSeconds ?? 0f;
        public float ActiveHoldRequiredSeconds => _activeHold?.RequiredSeconds ?? 0f;
        public bool IsChoosingAction => _menu != null;
        public IReadOnlyList<InteractionActionOption> ActionOptions =>
            _menu != null ? _menu.Options : Array.Empty<InteractionActionOption>();
        public int SelectedActionIndex => _menu?.SelectedIndex ?? 0;

        public PlayerInteraction(IInteractionInput input,
            IInteractionTargetFinder targetFinder, InteractionContext context)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _targetFinder = targetFinder ?? throw new ArgumentNullException(nameof(targetFinder));
            _context = context;
        }

        public void Tick(float deltaTime)
        {
            if (!_gameplayEnabled)
                return;

            if (_input.DropPressedThisFrame &&
                _context.Actor is IPickupReceiver receiver && receiver.IsOccupied)
            {
                CancelActiveInteraction(InteractionCancelReason.Interrupted);
                if (receiver.TryDrop())
                {
                    ClearFocus();
                    return;
                }
            }

            RefreshFocus();

            if (_menu != null)
            {
                UpdateMenu(deltaTime);
                return;
            }

            if (IsHolding)
            {
                if (_input.InteractionMenuCancelPressed)
                    CancelActiveInteraction(InteractionCancelReason.Interrupted);
                else
                    UpdateActiveHold(deltaTime);
                return;
            }

            if (!_input.InteractPressedThisFrame || _focusedActions.Count == 0)
                return;

            if (_focusedActions.Count > 1)
            {
                _menu = new InteractionActionMenu(FocusedTarget, _focusedActions, _context);
                _menuInputRevision = _input.ContextRevision;
                _awaitingMenuRelease = true;
                _input.SetInteractionMenuOpen(true);
                return;
            }

            StartAction(FocusedTarget, _focusedActions[0]);
            if (IsHolding)
                UpdateActiveHold(deltaTime);
        }

        public bool TryRestrictTargetScope(IInteractionScope scope)
        {
            if (scope == null) return false;
            if (ReferenceEquals(_targetScope, scope)) return true;
            CancelActiveInteraction(InteractionCancelReason.ContextChanged);
            ClearFocus();
            _targetScope = scope;
            return true;
        }

        public void ClearTargetScope()
        {
            if (_targetScope == null) return;
            CancelActiveInteraction(InteractionCancelReason.ContextChanged);
            ClearFocus();
            _targetScope = null;
        }

        public void CancelActiveInteraction(InteractionCancelReason reason)
        {
            CloseMenu(true);
            HoldInteractionSession session = _activeHold;
            _activeHold = null;
            if (session != null)
                session.Action.Cancel(_context, reason);
        }

        public void SetGameplayEnabled(bool isEnabled)
        {
            if (_gameplayEnabled == isEnabled) return;
            _gameplayEnabled = isEnabled;
            if (isEnabled) return;
            CancelActiveInteraction(InteractionCancelReason.ContextChanged);
            ClearFocus();
        }

        private void UpdateMenu(float deltaTime)
        {
            if (_input.InteractionMenuCancelPressed || _input.ContextRevision != _menuInputRevision ||
                !_menu.CanContinue(FocusedTarget, _focusedActions, _context))
            {
                CloseMenu(true);
                return;
            }

            _menu.Move(_input.InteractionMenuNavigation);
            if (_awaitingMenuRelease)
            {
                if (!_input.InteractHeld)
                    _awaitingMenuRelease = false;
                return;
            }

            if (!_input.InteractPressedThisFrame)
                return;

            IInteractionTarget target = _menu.Target;
            IInteractionAction action = _menu.SelectedAction;
            CloseMenu(false);
            StartAction(target, action);
            if (IsHolding)
                UpdateActiveHold(deltaTime);
        }

        private void CloseMenu(bool suppressInput)
        {
            if (_menu == null) return;
            _menu = null;
            _awaitingMenuRelease = false;
            _input.SetInteractionMenuOpen(false);
            if (suppressInput)
                _input.SuppressInteractionUntilReleased();
        }

        private void StartAction(IInteractionTarget target, IInteractionAction action)
        {
            if (!_gameplayEnabled || target == null || !target.IsActive ||
                !ReferenceEquals(FocusedTarget, target) ||
                !InteractionActions.Contains(_focusedActions, action) ||
                (_targetScope != null && !_targetScope.Allows(target)))
                return;

            InteractionInfo info = action.GetInteractionInfo(_context);
            if (!info.IsAvailable)
                return;

            long targetRevision = target.Revision;
            long actionRevision = action.Revision;
            if (!action.Begin(_context))
            {
                RefreshFocus();
                return;
            }

            if (!_gameplayEnabled || !target.IsActive || target.Revision != targetRevision ||
                action.Revision != actionRevision ||
                (_targetScope != null && !_targetScope.Allows(target)))
            {
                action.Cancel(_context, InteractionCancelReason.ContextChanged);
                return;
            }

            if (info.Mode == InteractionMode.Press)
            {
                action.Complete(_context);
                if (_gameplayEnabled)
                    RefreshFocus();
                return;
            }

            _activeHold = new HoldInteractionSession(target, action, info);
            FocusedInfo = action.GetInteractionInfo(_context);
        }

        private void UpdateActiveHold(float deltaTime)
        {
            if (_input.InteractReleasedThisFrame || !_input.InteractHeld)
            {
                CancelActiveInteraction(InteractionCancelReason.InputReleased);
                return;
            }

            HoldInteractionSession session = _activeHold;
            if (!HasFocusedTarget || !ReferenceEquals(FocusedTarget, session.Target))
            {
                CancelActiveInteraction(InteractionCancelReason.TargetLost);
                return;
            }

            if (!session.Target.IsActive || session.Target.Revision != session.TargetRevision ||
                session.Action.Revision != session.ActionRevision ||
                !InteractionActions.Contains(_focusedActions, session.Action) ||
                !session.Info.Matches(session.Action.GetInteractionInfo(_context)))
            {
                CancelActiveInteraction(InteractionCancelReason.TargetUnavailable);
                return;
            }

            session.Advance(deltaTime);
            if (!session.IsComplete) return;

            _activeHold = null;
            session.Action.Complete(_context);
            if (_gameplayEnabled)
                RefreshFocus();
        }

        private void RefreshFocus()
        {
            if (!_targetFinder.TryFindTarget(_targetScope, _context, out IInteractionTarget target))
            {
                ClearFocus();
                return;
            }

            InteractionActions.CollectAvailable(target, _context, _focusedActions);
            if (_focusedActions.Count == 0)
            {
                ClearFocus();
                return;
            }

            FocusedTarget = target;
            HasFocusedTarget = true;
            if (_activeHold != null && ReferenceEquals(target, _activeHold.Target))
                FocusedInfo = _activeHold.Action.GetInteractionInfo(_context);
            else
                FocusedInfo = _focusedActions.Count == 1
                    ? _focusedActions[0].GetInteractionInfo(_context)
                    : InteractionInfo.Press(InteractionPromts.Choose);
        }

        private void ClearFocus()
        {
            FocusedTarget = null;
            FocusedInfo = default;
            HasFocusedTarget = false;
            _focusedActions.Clear();
        }
    }
}

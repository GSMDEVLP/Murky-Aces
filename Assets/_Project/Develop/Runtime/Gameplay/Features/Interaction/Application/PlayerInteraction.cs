using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class PlayerInteraction : ITickable
    {
        private readonly PlayerIntentBuffer _intent;
        private readonly IInteractionTargetFinder _targetFinder;
        private readonly InteractionContext _context;
        private IInteractionScope _targetScope;

        private HoldInteractionSession _activeHold;

        public bool HasFocusedTarget { get; private set; }
        public IInteractable FocusedTarget { get; private set; }
        public InteractionInfo FocusedInfo { get; private set; }

        public bool IsHolding =>_activeHold != null;
        
        public float ActiveHoldElapsedSeconds => _activeHold?.ElapsedSeconds ?? 0f;
        public float ActiveHoldRequiredSeconds => _activeHold?.RequiredSeconds ?? 0f;

        private bool _gameplayEnabled = true;
        public bool IsGameplayEnabled => _gameplayEnabled;

        public PlayerInteraction(PlayerIntentBuffer intent, IInteractionTargetFinder targetFinder, InteractionContext context)
        {
            _intent = intent;
            _targetFinder = targetFinder;
            _context = context;
        }

        public void Tick(float deltaTime)
        {
            if (!_gameplayEnabled)
                return;
            if (_intent.DropPressedThisFrame && TryDropHeldItem())
            {
                return;
            }

            RefreshFocus();

            if (IsHolding)
            {
                UpdateActiveHold(deltaTime);
                return;
            }

            if (!_intent.InteractPressedThisFrame)
                return;

            TryStartFocusedInteraction();

            if (IsHolding)
                UpdateActiveHold(deltaTime);
        }

        public bool TryRestrictTargetScope(IInteractionScope scope)
        {
            if (scope == null)
                return false;

            if (ReferenceEquals(
                    _targetScope,
                    scope))
            {
                return true;
            }

            CancelActiveInteraction(
                InteractionCancelReason.ContextChanged);

            ClearFocus();
            _targetScope = scope;

            return true;
        }

        public void ClearTargetScope()
        {
            if (_targetScope == null)
                return;

            CancelActiveInteraction(
                InteractionCancelReason.ContextChanged);

            ClearFocus();
            _targetScope = null;
        }
        public void CancelActiveInteraction(InteractionCancelReason reason)
        {
            if (!IsHolding)
                return;

            HoldInteractionSession session =
                _activeHold;

            _activeHold = null;

            session.Target.Cancel(_context, reason);
        }

        private void TryStartFocusedInteraction()
        {
            if (!HasFocusedTarget)
                return;

            if (!FocusedInfo.IsAvailable)
                return;

            if (!FocusedTarget.Begin(_context))
            {
                RefreshFocus();
                return;
            }

            if (FocusedInfo.Mode == InteractionMode.Press)
            {
                FocusedTarget.Complete(_context);
                RefreshFocus();
                return;
            }

            _activeHold = new HoldInteractionSession(
                FocusedTarget,
                FocusedInfo);
        }

        private bool TryDropHeldItem()
        {
            IPickupReceiver pickupReceiver = _context.Actor as IPickupReceiver;

            if (pickupReceiver == null)
                return false;

            if (!pickupReceiver.TryDrop())
                return false;

            ClearFocus();

            return true;
        }

        private void UpdateActiveHold(float deltaTime)
        {
            if (_intent.InteractReleasedThisFrame ||
                !_intent.InteractHeld)
            {
                CancelActiveInteraction(InteractionCancelReason.InputReleased);

                return;
            }

            if (!HasFocusedTarget ||
                !object.ReferenceEquals(
                    FocusedTarget,
                    _activeHold.Target))
            {
                CancelActiveInteraction(
                    InteractionCancelReason.TargetLost);

                return;
            }

            if (!FocusedInfo.IsAvailable ||
                FocusedInfo.Mode != InteractionMode.Hold)
            {
                CancelActiveInteraction(
                    InteractionCancelReason.TargetUnavailable);

                return;
            }

            _activeHold.Advance(deltaTime);

            if (_activeHold.IsComplete)
                CompleteActiveHold();
        }

        private void CompleteActiveHold()
        {
            HoldInteractionSession session =
                _activeHold;

            _activeHold = null;

            session.Target.Complete(_context);

            RefreshFocus();
        }

        private void RefreshFocus()
        {
            IPickupReceiver pickupReceiver = _context.Actor as IPickupReceiver;

            if (pickupReceiver != null && pickupReceiver.IsOccupied)
            {
                ClearFocus();
                return;
            }

            if (!_targetFinder.TryFindTarget(
                    _targetScope,
                    _context,
                    out IInteractable target))
            {
                ClearFocus();
                return;
            }

            FocusedTarget = target;
            FocusedInfo = target.GetInteractionInfo(_context);

            HasFocusedTarget = true;
        }

        public void SetGameplayEnabled(bool isEnabled)
        {
            if (_gameplayEnabled == isEnabled)
                return;

            _gameplayEnabled = isEnabled;

            if (isEnabled)
                return;

            CancelActiveInteraction(
                InteractionCancelReason.ContextChanged);

            ClearFocus();
        }

        private void ClearFocus()
        {
            FocusedTarget = null;
            FocusedInfo = default;
            HasFocusedTarget = false;
        }
    }
}

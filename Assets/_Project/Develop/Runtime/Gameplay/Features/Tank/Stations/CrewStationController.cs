using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationController : IGameplayTickable
    {
        private readonly CrewStationOccupancy _station;
        private readonly CrewStationView  _view;
        private readonly ICrewStationRoleAdapter _roleAdapter;
        private readonly StationDisplayFeed _displayFeed;

        private IStationOccupant _currentOccupant;
        private IStationIntentSource _stationIntentSource;


        public CrewRoleId RoleId => _view.RoleId;
        public CrewStationState State => _station.State;
        public bool IsOccupied => _station.IsOccupied;
        
        public CrewStationController(
            CrewStationOccupancy station,
            CrewStationView  view,
            ICrewStationRoleAdapter roleAdapter,
            StationDisplayFeed displayFeed)
        {
            _station = station ??
                throw new ArgumentNullException(
                    nameof(station));

            _view = view ??
                throw new ArgumentNullException(
                    nameof(view));

            _roleAdapter = roleAdapter ??
                throw new ArgumentNullException(
                    nameof(roleAdapter));

            _displayFeed = displayFeed ??
                throw new ArgumentNullException(nameof(displayFeed));
        }

        public bool CanBeginEnter(IInteractionActor actor)
        {
            return actor != null &&
                   _station.IsFree &&
                   _view.CanEnter &&
                   _roleAdapter.IsActive == false &&
                   CanActorUseStation(actor);
        }

        public bool CanContinueEnter(IInteractionActor actor)
        {
            return actor != null &&
                   _station.State == CrewStationState.Entering &&
                   _station.OccupantId == actor.Id &&
                   _view.CanEnter &&
                   _roleAdapter.IsActive == false &&
                   CanActorUseStation(actor);
        }

        public bool TryBeginEnter(IInteractionActor actor)
        {
            return CanBeginEnter(actor) &&
                   _station.TryBeginEnter(actor.Id);
        }

        public bool TryCompleteEnter(IInteractionActor actor)
        {
            if (CanContinueEnter(actor) == false)
                return false;

            IStationOccupant occupant = (IStationOccupant)actor;

            if (occupant.TryEnterStation(
                    _view.SeatAnchor,
                    _view.CameraAnchor,
                    _view.PanelRoot,
                    _view.CapabilityProfile) == false)
            {
                return CancelFailedEnter(actor.Id);
            }

            if (_roleAdapter.TryActivate(actor) == false)
            {
                return RollbackEnter(
                    actor.Id,
                    occupant,
                    deactivateRole: false);
            }

            if (occupant.TrySetReleaseHandler(TryForceReleaseCurrentOccupant) == false)
            {
                return RollbackEnter(
                    actor.Id,
                    occupant,
                    deactivateRole: true);
            }

            if (_station.TryCompleteEnter(actor.Id) == false)
            {
                occupant.ClearReleaseHandler(TryForceReleaseCurrentOccupant);
                return RollbackEnter(
                    actor.Id,
                    occupant,
                    deactivateRole: true);
            }

            _currentOccupant = occupant;
            _stationIntentSource = (IStationIntentSource)actor;
            _displayFeed.Activate();

            return true;
        }

        public bool TryCancelEnter(IInteractionActor actor)
        {
            if (actor == null)
                return false;

            bool wasCancelled =
                _station.TryCancelEnter(actor.Id);

            if (wasCancelled)
                _roleAdapter.ClearOutput();

            return wasCancelled;
        }

        public void Tick(float deltaTime)
        {
            if (HasActiveOccupant() == false)
            {
                _roleAdapter.ClearOutput();
                return;
            }

            if (_stationIntentSource
                .ConsumeExitRequest())
            {
                TryExitCurrentOccupant();
                return;
            }

            _roleAdapter.Tick(deltaTime);
        }

        public void ClearOutput()
        {
            _roleAdapter.ClearOutput();
        }

        private bool TryExitCurrentOccupant(bool force = false)
        {
            if (HasActiveOccupant() == false ||
                _view.CanExit == false)
            {
                return false;
            }

            ulong occupantId = _station.OccupantId.Value;

            if (_station.TryBeginExit(occupantId) == false)
                return false;

            _roleAdapter.ClearOutput();

            bool exited = force
                ? _currentOccupant.TryForceExitStation(_view.ExitAnchor)
                : _currentOccupant.TryExitStation(_view.ExitAnchor);

            if (exited == false)
            {
                RestoreOccupiedState(occupantId);
                return false;
            }

            if (_station.TryCompleteExit(occupantId) == false)
            {
                RollbackExit(occupantId);
                return false;
            }

            _currentOccupant.ClearReleaseHandler(TryForceReleaseCurrentOccupant);
            _displayFeed.Deactivate();
            _roleAdapter.Deactivate();

            _currentOccupant = null;
            _stationIntentSource = null;

            return true;
        }

        private bool TryForceReleaseCurrentOccupant()
        {
            return TryExitCurrentOccupant(force: true);
        }

        private bool CanActorUseStation(IInteractionActor actor)
        {
            IStationOccupant occupant = actor as IStationOccupant;

            return occupant != null &&
                   occupant.IsInStation == false &&
                   actor is IStationIntentSource &&
                   _roleAdapter.CanUse(actor);
        }

        private bool HasActiveOccupant()
        {
            return _currentOccupant != null &&
                   _stationIntentSource != null &&
                   _station.OccupantId.HasValue &&
                   _station.IsOccupiedBy(
                       _station.OccupantId.Value);
        }

        private bool CancelFailedEnter(ulong occupantId)
        {
            _roleAdapter.ClearOutput();

            if (_station.TryCancelEnter(occupantId))
                return false;

            throw new InvalidOperationException(
                "Crew station failed to cancel enter.");
        }

        private bool RollbackEnter(ulong occupantId, IStationOccupant occupant, bool deactivateRole)
        {
            if (deactivateRole)
                _roleAdapter.Deactivate();
            else
                _roleAdapter.ClearOutput();

            bool occupantRolledBack = occupant.TryForceExitStation(_view.ExitAnchor);

            bool stationRolledBack = _station.TryCancelEnter(occupantId);

            if (occupantRolledBack && stationRolledBack)
            {
                return false;
            }

            throw new InvalidOperationException(
                "Crew station enter rollback failed.");
        }

        private void RestoreOccupiedState(
            ulong occupantId)
        {
            if (_station.TryCancelExit(occupantId))
                return;

            throw new InvalidOperationException(
                "Crew station failed to cancel exit.");
        }

        private void RollbackExit(ulong occupantId)
        {
            bool occupantRolledBack =
                _currentOccupant.TryEnterStation(
                    _view.SeatAnchor,
                    _view.CameraAnchor,
                    _view.PanelRoot,
                    _view.CapabilityProfile);

            bool stationRolledBack = _station.TryCancelExit(occupantId);

            if (occupantRolledBack && stationRolledBack)
            {
                return;
            }

            throw new InvalidOperationException("Crew station exit rollback failed.");
        }
    }
}
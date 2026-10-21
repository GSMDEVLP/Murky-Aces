using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstraction;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application
{
    public sealed class CrewStationController : IGameplayTickable
    {
        private readonly CrewStationOccupancy _station;
        private readonly ICrewStationContext _context;
        private readonly ICrewStationRoleAdapter _roleAdapter;

        private IStationOccupant _currentOccupant;
        private IStationIntentSource _stationIntentSource;


        public CrewRoleId RoleId => _context.RoleId;
        public CrewStationState State => _station.State;
        public bool IsOccupied => _station.IsOccupied;
        private long? _entryRevision;
        
        public CrewStationController(
            CrewStationOccupancy station,
            ICrewStationContext context,
            ICrewStationRoleAdapter roleAdapter)
        {
            _station = station ??
                throw new ArgumentNullException(nameof(station));

            _context = context ??
                throw new ArgumentNullException(nameof(context));

            _roleAdapter = roleAdapter ??
                throw new ArgumentNullException(nameof(roleAdapter));
        }
        public bool IsOccupiedBy(ulong occupantId)
        {
            return HasActiveOccupant() &&
                _station.IsOccupiedBy(occupantId);
        }

        public bool CanBeginEnter(IInteractionActor actor)
        {
            return actor != null &&
                   _station.IsFree &&
                   _context.CanEnter &&
                   _roleAdapter.IsActive == false &&
                   CanActorUseStation(actor);
        }

        public bool CanContinueEnter(IInteractionActor actor)
        {
            return actor != null &&
                _station.State == CrewStationState.Entering &&
                _station.OccupantId == actor.Id &&
                _context.CanEnter &&
                !_roleAdapter.IsActive &&
                CanActorUseStation(actor) &&
                _entryRevision.HasValue &&
                ((ICrewLocationReader)actor).Revision == _entryRevision.Value;
        }

        public bool TryBeginEnter(IInteractionActor actor)
        {
            if (!CanBeginEnter(actor) ||
                !_station.TryBeginEnter(actor.Id))
            {
                return false;
            }

            _entryRevision = ((ICrewLocationReader)actor).Revision;
            return true;
        }

        public bool TryCompleteEnter(IInteractionActor actor)
        {
            if (!CanContinueEnter(actor))
            {
                if (actor != null &&
                    _station.State == CrewStationState.Entering &&
                    _station.OccupantId == actor.Id)
                {
                    TryCancelEnter(actor);
                }

                return false;
            }

            IStationOccupant occupant = (IStationOccupant)actor;

            if (!occupant.TryEnterStation(
                    _entryRevision.Value,
                    _context.RoleId,
                    _context.SeatAnchor,
                    _context.CameraAnchor,
                    _context.InteractionScope,
                    _context.CapabilityProfile))
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

            _entryRevision = null;
            _currentOccupant = occupant;
            _stationIntentSource = (IStationIntentSource)actor;

            return true;
        }

        public bool TryCancelEnter(IInteractionActor actor)
        {
            if (actor == null)
                return false;

            bool wasCancelled =
                _station.TryCancelEnter(actor.Id);

            if (wasCancelled)
            {
                _entryRevision = null;
                _roleAdapter.ClearOutput();
            }

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

        public bool CanExit(IInteractionActor actor)
        {
            return actor != null &&
                HasActiveOccupant() &&
                _station.IsOccupiedBy(actor.Id) &&
                _context.CanExit;
        }

        public bool TryExit(IInteractionActor actor)
        {
            return CanExit(actor) &&
                TryExitCurrentOccupant();
        }

        private bool TryExitCurrentOccupant(bool force = false)
        {
            if (HasActiveOccupant() == false ||
                _context.CanExit == false)
            {
                return false;
            }

            ulong occupantId = _station.OccupantId.Value;

            if (_station.TryBeginExit(occupantId) == false)
                return false;

            _roleAdapter.ClearOutput();

            bool exited = force
                ? _currentOccupant.TryForceExitStation(_context.ExitAnchor)
                : _currentOccupant.TryExitStation(_context.ExitAnchor);

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
            var occupant = actor as IStationOccupant;
            var location = actor as ICrewLocationReader;

            return occupant != null &&
                !occupant.IsInStation &&
                location != null &&
                location.Current.Kind == _context.RequiredEntryLocation &&
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
            _entryRevision = null;
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

            bool occupantRolledBack = occupant.TryRollbackStationEntry();

            bool stationRolledBack = _station.TryCancelEnter(occupantId);
            
            if (stationRolledBack)
                _entryRevision = null;

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
                    ((ICrewLocationReader)_currentOccupant).Revision,
                    _context.RoleId,
                    _context.SeatAnchor,
                    _context.CameraAnchor,
                    _context.InteractionScope,
                    _context.CapabilityProfile);

            bool stationRolledBack = _station.TryCancelExit(occupantId);

            if (occupantRolledBack && stationRolledBack)
            {
                return;
            }

            throw new InvalidOperationException("Crew station exit rollback failed.");
        }
    }
}
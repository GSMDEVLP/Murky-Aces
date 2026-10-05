using System;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Crew.Application
{
    public sealed class CrewTransitionCoordinator : ICrewLocationReader
    {
        private readonly CrewLocationState _state = new CrewLocationState();

        private bool _isTransitioning;

        public CrewLocation Current => _state.Current;
        public long Revision => _state.Revision;

        public bool CanTransitionTo(CrewLocation destination)
        {
            return !_isTransitioning &&
                   _state.CanTransitionTo(destination);
        }

        public bool TryTransition(
            long expectedRevision,
            CrewLocation destination,
            Func<bool> tryApply,
            Func<bool> tryRollback)
        {
            if (tryApply == null)
                throw new ArgumentNullException(nameof(tryApply));

            if (tryRollback == null)
                throw new ArgumentNullException(nameof(tryRollback));

            if (Revision != expectedRevision ||
                !CanTransitionTo(destination))
            {
                return false;
            }

            _isTransitioning = true;

            try
            {
                bool applied;

                try
                {
                    applied = tryApply();
                }
                catch (Exception failure)
                {
                    RollbackOrThrow(tryRollback, failure);
                    throw;
                }

                if (!applied)
                {
                    RollbackOrThrow(tryRollback, null);
                    return false;
                }

                if (!_state.TryCommit(expectedRevision, destination))
                {
                    var failure = new InvalidOperationException(
                        "Crew location changed during a transition.");

                    RollbackOrThrow(tryRollback, failure);
                    throw failure;
                }

                return true;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private static void RollbackOrThrow(
            Func<bool> tryRollback,
            Exception failure)
        {
            bool restored;

            try
            {
                restored = tryRollback();
            }
            catch (Exception rollbackFailure)
            {
                Exception cause = failure == null
                    ? rollbackFailure
                    : new AggregateException(failure, rollbackFailure);

                throw new InvalidOperationException(
                    "Crew transition rollback threw an exception.",
                    cause);
            }

            if (!restored)
            {
                throw new InvalidOperationException(
                    "Crew transition rollback failed.",
                    failure);
            }
        }
    }
}
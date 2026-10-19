using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public sealed class CarrySession
    {
        private readonly List<ulong> _participants = new();
        private readonly IReadOnlyList<ulong> _participantsView;

        public IReadOnlyList<ulong> Participants =>
            _participantsView;

        public int ParticipantCount => _participants.Count;

        public int MaxParticipants { get; }

        public long Revision { get; private set; }

        public CarrySession(int maxParticipants)
        {
            if (maxParticipants < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxParticipants));
            }

            MaxParticipants = maxParticipants;
            _participantsView = _participants.AsReadOnly();
        }

        public bool Contains(ulong actorId)
        {
            return _participants.Contains(actorId);
        }

        public bool TryAddParticipant(ulong actorId)
        {
            if (Contains(actorId) ||
                ParticipantCount >= MaxParticipants)
            {
                return false;
            }

            _participants.Add(actorId);
            Revision++;

            return true;
        }

        public bool TryRemoveParticipant(ulong actorId)
        {
            if (!_participants.Remove(actorId))
                return false;

            Revision++;
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure
{
    public sealed class ActiveCommonActionsReader
    {
        private readonly IReadOnlyList<ICommonActionsReader> _readers;

        public ActiveCommonActionsReader(List<ICommonActionsReader> readers)
        {
            if (readers == null)
            {
                throw new ArgumentNullException(
                    nameof(readers));
            }

            if (readers.Count == 0)
            {
                throw new ArgumentException(
                    "At least one common actions reader is required.",
                    nameof(readers));
            }

            ValidateReaders(readers);

            _readers = readers.AsReadOnly();
        }

        public CommonInputSnapshot ReadCommonActions()
        {
            ICommonActionsReader activeReader = null;

            for (int i = 0; i < _readers.Count; i++)
            {
                ICommonActionsReader reader =
                    _readers[i];

                if (reader.IsActive == false)
                    continue;

                if (activeReader != null)
                {
                    throw new InvalidOperationException(
                        "Multiple input map readers are active.");
                }

                activeReader = reader;
            }

            return activeReader != null
                ? activeReader.ReadCommonActions()
                : CommonInputSnapshot.Neutral;
        }

        private static void ValidateReaders(
            List<ICommonActionsReader> readers)
        {
            HashSet<InputMapId> registeredMaps =
                new HashSet<InputMapId>();

            for (int i = 0; i < readers.Count; i++)
            {
                ICommonActionsReader reader =
                    readers[i];

                if (reader == null)
                {
                    throw new ArgumentException(
                        "Common actions reader cannot be null.",
                        nameof(readers));
                }

                if (reader.MapId == InputMapId.None)
                {
                    throw new ArgumentException(
                        "Common actions reader cannot use None map.",
                        nameof(readers));
                }

                if (registeredMaps.Add(reader.MapId) == false)
                {
                    throw new InvalidOperationException(
                        $"Duplicate common actions reader for {reader.MapId}.");
                }
            }
        }
    }
}
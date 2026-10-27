using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters
{
    public sealed class GpsInputUpdater : IInputIntentUpdater
    {
        private readonly GpsActionsReader _reader;
        private readonly GpsIntentBuffer _intent;

        public GpsInputUpdater(
            GpsActionsReader reader,
            GpsIntentBuffer intent)
        {
            _reader = reader ??
                throw new ArgumentNullException(nameof(reader));

            _intent = intent ??
                throw new ArgumentNullException(nameof(intent));
        }

        public void UpdateIntent()
        {
            _intent.SetZoom(_reader.ReadZoom());
            _intent.SetCursorDelta(_reader.ReadCursorDelta());

            _intent.SetSelectionInput(
                _reader.ReadSelectionPressed(),
                _reader.ReadSelectionHeld());

            _intent.SetClearInput(
                _reader.ReadClearPressed(),
                _reader.ReadClearHeld());
        }
    }
}

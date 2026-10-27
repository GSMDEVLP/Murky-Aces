using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerGpsInputHandler
    {
        private readonly GpsIntentBuffer _intent;

        private Vector2 _cursorUv = new Vector2(0.5f, 0.5f);

        public PlayerGpsInputHandler(GpsIntentBuffer intent)
        {
            _intent = intent ??
                throw new ArgumentNullException(nameof(intent));
        }

        public GpsIntentSnapshot ConsumeInput()
        {
            bool selectionRequested = _intent.ConsumeSelectionRequest();
            bool clearRequested = _intent.ConsumeClearRequest();
            float zoom = _intent.ConsumeZoom();
            Vector2 cursorDelta = _intent.ConsumeCursorDelta();

            return new GpsIntentSnapshot(
                zoom,
                cursorDelta,
                selectionRequested,
                clearRequested);
        }

        public void ResetCursor()
        {
            _cursorUv = new Vector2(0.5f, 0.5f);
        }

        public void ClearInput()
        {
            _intent.Clear();
        }

        public void HandleInput(
            IGpsScreen screen,
            IInteractionActor actor,
            GpsIntentSnapshot input)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));

            if (actor == null)
                throw new ArgumentNullException(nameof(actor));

            if (input.Zoom != 0f)
                screen.TryZoom(actor, input.Zoom);

            if (screen.Role != CrewRoleId.CommanderLoader)
                return;

            float sensitivity = screen.CursorSensitivity;
            float aspect = screen.ScreenAspect;

            _cursorUv = new Vector2(
                Mathf.Clamp01(
                    _cursorUv.x +
                    input.CursorDelta.x * sensitivity / aspect),
                Mathf.Clamp01(
                    _cursorUv.y +
                    input.CursorDelta.y * sensitivity));

            screen.TrySetCursor(actor, _cursorUv);

            if (input.ClearRequested)
            {
                screen.TryClearRoute(actor);
            }
            else if (input.SelectionRequested)
            {
                screen.TrySelectDestination(actor, _cursorUv);
            }
        }
    }
}

using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Domain
{
    public sealed class GpsIntentBuffer
    {
        private float _zoom;
        private Vector2 _cursorDelta;
        private bool _selectionPending;
        private bool _selectionSuppressed;
        private bool _clearPending;
        private bool _clearSuppressed;

        public void SetZoom(float zoom)
        {
            _zoom = zoom;
        }

        public void SetCursorDelta(Vector2 cursorDelta)
        {
            _cursorDelta = cursorDelta;
        }

        public float ConsumeZoom()
        {
            float zoom = _zoom;
            _zoom = 0f;

            return zoom;
        }

        public Vector2 ConsumeCursorDelta()
        {
            Vector2 delta = _cursorDelta;
            _cursorDelta = Vector2.zero;

            return delta;
        }

        public void Clear()
        {
            _zoom = 0f;
            _cursorDelta = Vector2.zero;
            _selectionPending = false;
            _selectionSuppressed = true;
            _clearPending = false;
            _clearSuppressed = true;
        }

        public void SetSelectionInput(bool pressedThisFrame, bool held)
        {
            if (_selectionSuppressed)
            {
                if (!held)
                    _selectionSuppressed = false;

                return;
            }

            _selectionPending |= pressedThisFrame;
        }

        public bool ConsumeSelectionRequest()
        {
            bool requested = _selectionPending;
            _selectionPending = false;

            return requested;
        }
        public void SetClearInput(bool pressedThisFrame, bool held)
        {
            if (_clearSuppressed)
            {
                if (!held)
                    _clearSuppressed = false;

                return;
            }

            _clearPending |= pressedThisFrame;
        }

        public bool ConsumeClearRequest()
        {
            bool requested = _clearPending;
            _clearPending = false;

            return requested;
        }
    }
}
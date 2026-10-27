using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct GpsIntentSnapshot
    {
        public float Zoom { get; }
        public Vector2 CursorDelta { get; }
        public bool SelectionRequested { get; }
        public bool ClearRequested { get; }

        public GpsIntentSnapshot(
            float zoom,
            Vector2 cursorDelta,
            bool selectionRequested,
            bool clearRequested)
        {
            Zoom = zoom;
            CursorDelta = cursorDelta;
            SelectionRequested = selectionRequested;
            ClearRequested = clearRequested;
        }
    }
}

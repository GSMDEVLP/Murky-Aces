using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Domain
{
    public sealed class GunnerIntentBuffer
    {
        private float _traverse;
        private float _elevation;
        private bool _zoomHeld;

        private bool _fireRequested;
        private bool _toggleRequested;
        private bool _exitRequested;

        private bool _fireSuppressed;
        private bool _toggleSuppressed;
        private bool _exitSuppressed;

        public void SetActions(GunnerActionsSnapshot actions)
        {
            _traverse = ClampAxis(actions.AimX);
            _elevation = ClampAxis(actions.AimY);
            _zoomHeld = actions.ZoomHeld;

            if (_fireSuppressed)
            {
                if (!actions.FireHeld)
                    _fireSuppressed = false;
            }
            else if (actions.FirePressedThisFrame)
            {
                _fireRequested = true;
            }

            if (_toggleSuppressed)
            {
                if (!actions.ToggleHeld)
                    _toggleSuppressed = false;
            }
            else if (actions.TogglePressedThisFrame)
            {
                _toggleRequested = true;
            }

            if (_exitSuppressed)
            {
                if (!actions.ExitHeld)
                    _exitSuppressed = false;
            }
            else if (actions.ExitPressedThisFrame)
            {
                _exitRequested = true;
            }
        }

        public GunnerIntentSnapshot ConsumeIntent()
        {
            GunnerIntentSnapshot snapshot =
                new GunnerIntentSnapshot(
                    _traverse,
                    _elevation,
                    _zoomHeld,
                    _fireRequested,
                    _toggleRequested);

            _fireRequested = false;
            _toggleRequested = false;
            return snapshot;
        }

        public bool ConsumeExitRequest()
        {
            bool requested = _exitRequested;
            _exitRequested = false;
            return requested;
        }

        public void SuppressButtonsUntilReleased()
        {
            _fireRequested = false;
            _toggleRequested = false;
            _exitRequested = false;

            _fireSuppressed = true;
            _toggleSuppressed = true;
            _exitSuppressed = true;
        }

        public void Clear()
        {
            _traverse = 0f;
            _elevation = 0f;
            _zoomHeld = false;

            _fireRequested = false;
            _toggleRequested = false;
            _exitRequested = false;

            _fireSuppressed = false;
            _toggleSuppressed = false;
            _exitSuppressed = false;
        }

        private static float ClampAxis(float value)
        {
            if (value < -1f) return -1f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
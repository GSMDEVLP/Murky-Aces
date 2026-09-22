namespace _Project.Develop.Runtime.Gameplay.Features.Player.Domain
{
    public sealed class DrivingIntentBuffer
    {
        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsBraking { get; private set; }

        private bool _exitRequested;
        private bool _isExitSuppressed;

        public void SetDriving(float throttle, float steering, bool isBraking)
        {
            Throttle = ClampAxis(throttle);
            Steering = ClampAxis(steering);
            IsBraking = isBraking;
        }

        public void SetExitInput(bool pressedThisFrame, bool held)
        {
            if (_isExitSuppressed)
            {
                if (held == false)
                    _isExitSuppressed = false;

                return;
            }

            if (pressedThisFrame)
                _exitRequested = true;
        }

        public void SuppressExitUntilReleased()
        {
            _isExitSuppressed = true;
            _exitRequested = false;
        }

        public bool ConsumeExitRequest()
        {
            bool requested = _exitRequested;
            _exitRequested = false;

            return requested;
        }

        public void Clear()
        {
            Throttle = 0f;
            Steering = 0f;
            IsBraking = false;
            _exitRequested = false;
            _isExitSuppressed = false;
        }

        private static float ClampAxis(float value)
        {
            if (value < -1f)
                return -1f;

            if (value > 1f)
                return 1f;

            return value;
        }
    }
}
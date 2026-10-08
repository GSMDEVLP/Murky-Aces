namespace _Project.Develop.Runtime.Gameplay.Features.Player.Domain
{
    public sealed class CommanderIntentBuffer
    {
        private bool _exitRequested;
        private bool _exitSuppressed;

        public void SetExitInput(bool pressedThisFrame, bool held)
        {
            if (_exitSuppressed)
            {
                if (!held)
                    _exitSuppressed = false;

                return;
            }

            if (pressedThisFrame)
                _exitRequested = true;
        }

        public bool ConsumeExitRequest()
        {
            bool requested = _exitRequested;
            _exitRequested = false;
            return requested;
        }

        public void SuppressExitUntilReleased()
        {
            _exitRequested = false;
            _exitSuppressed = true;
        }

        public void Clear()
        {
            _exitRequested = false;
            _exitSuppressed = false;
        }
    }
}
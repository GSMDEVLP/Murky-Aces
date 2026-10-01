using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret
{
    public sealed class TurretMechanism
    {
        private readonly TurretAimState _state;
        private readonly float _traverseDegreesPerSecond;
        private readonly float _elevationDegreesPerSecond;
        private readonly float _minPitch;
        private readonly float _maxPitch;

        private float _traverseInput;
        private float _elevationInput;
        private float _lastHullYaw;

        public TurretAimState State => _state;

        public TurretMechanism(
            TurretAimState state,
            float traverseDegreesPerSecond,
            float elevationDegreesPerSecond,
            float minPitch,
            float maxPitch)
        {
            _state = state ??
                throw new ArgumentNullException(nameof(state));

            if (!IsFinite(traverseDegreesPerSecond) ||
                traverseDegreesPerSecond <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(traverseDegreesPerSecond));

            if (!IsFinite(elevationDegreesPerSecond) ||
                elevationDegreesPerSecond <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(elevationDegreesPerSecond));

            if (!IsFinite(minPitch) ||
                !IsFinite(maxPitch) ||
                minPitch > maxPitch)
                throw new ArgumentException(
                    "Invalid turret pitch limits.");

            _traverseDegreesPerSecond =
                traverseDegreesPerSecond;
            _elevationDegreesPerSecond =
                elevationDegreesPerSecond;
            _minPitch = minPitch;
            _maxPitch = maxPitch;

            _state.CurrentPitch =
                Clamp(_state.CurrentPitch, _minPitch, _maxPitch);
            _state.TargetPitch = _state.CurrentPitch;

            _lastHullYaw = NormalizeAngle(
                _state.WorldDirection - _state.CurrentYaw);
        }

        public void SetInput(float traverse, float elevation)
        {
            if (!IsFinite(traverse))
                throw new ArgumentOutOfRangeException(
                    nameof(traverse));

            if (!IsFinite(elevation))
                throw new ArgumentOutOfRangeException(
                    nameof(elevation));

            _traverseInput = Clamp(traverse, -1f, 1f);
            _elevationInput = Clamp(elevation, -1f, 1f);
        }

        public void ToggleReferenceMode(float hullYaw)
        {
            if (!IsFinite(hullYaw))
                throw new ArgumentOutOfRangeException(
                    nameof(hullYaw));

            _lastHullYaw = NormalizeAngle(hullYaw);

            _state.TargetYaw = _state.CurrentYaw;
            _state.WorldDirection = NormalizeAngle(
                _lastHullYaw + _state.CurrentYaw);

            _state.ReferenceMode =
                _state.ReferenceMode ==
                TurretReferenceMode.HullRelative
                    ? TurretReferenceMode.WorldStabilized
                    : TurretReferenceMode.HullRelative;
        }

        public void Tick(float deltaTime, float hullYaw)
        {
            if (!IsFinite(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime));

            if (!IsFinite(hullYaw))
                throw new ArgumentOutOfRangeException(
                    nameof(hullYaw));

            _lastHullYaw = NormalizeAngle(hullYaw);

            if (deltaTime == 0f)
                return;

            float yawStep =
                _traverseInput *
                _traverseDegreesPerSecond *
                deltaTime;

            if (_state.ReferenceMode ==
                TurretReferenceMode.WorldStabilized)
            {
                _state.WorldDirection =
                    NormalizeAngle(
                        _state.WorldDirection + yawStep);

                _state.TargetYaw = NormalizeAngle(
                    _state.WorldDirection - _lastHullYaw);
            }
            else
            {
                _state.TargetYaw =
                    NormalizeAngle(
                        _state.TargetYaw + yawStep);

                _state.WorldDirection =
                    NormalizeAngle(
                        _lastHullYaw + _state.TargetYaw);
            }

            _state.TargetPitch = Clamp(
                _state.TargetPitch +
                _elevationInput *
                _elevationDegreesPerSecond *
                deltaTime,
                _minPitch,
                _maxPitch);

            _state.CurrentYaw = MoveTowardsAngle(
                _state.CurrentYaw,
                _state.TargetYaw,
                _traverseDegreesPerSecond * deltaTime);

            _state.CurrentPitch = MoveTowards(
                _state.CurrentPitch,
                _state.TargetPitch,
                _elevationDegreesPerSecond * deltaTime);
        }

        public void ClearInput()
        {
            _traverseInput = 0f;
            _elevationInput = 0f;

            _state.TargetYaw = _state.CurrentYaw;
            _state.TargetPitch = _state.CurrentPitch;
            _state.WorldDirection = NormalizeAngle(
                _lastHullYaw + _state.CurrentYaw);
        }

        private static float MoveTowardsAngle(
            float current,
            float target,
            float maxStep)
        {
            float difference =
                NormalizeAngle(target - current);

            if (Math.Abs(difference) <= maxStep)
                return NormalizeAngle(target);

            return NormalizeAngle(
                current + Math.Sign(difference) * maxStep);
        }

        private static float MoveTowards(
            float current,
            float target,
            float maxStep)
        {
            float difference = target - current;

            if (Math.Abs(difference) <= maxStep)
                return target;

            return current +
                Math.Sign(difference) * maxStep;
        }

        private static float Clamp(
            float value,
            float minimum,
            float maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        private static float NormalizeAngle(float angle)
        {
            float result = angle % 360f;

            if (result > 180f)
                result -= 360f;
            else if (result <= -180f)
                result += 360f;

            return result;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}
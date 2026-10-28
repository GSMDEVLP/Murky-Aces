using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Domain
{
    public sealed class TurretMechanism
    {
        private readonly float _traverseDegreesPerSecond;
        private readonly float _elevationDegreesPerSecond;
        private readonly float _minPitch;
        private readonly float _maxPitch;

        private float _traverseInput;
        private float _elevationInput;
        private float _lastHullYaw;
        private float _currentYaw;
        private float _targetYaw;
        private float _currentPitch;
        private float _targetPitch;
        private TurretReferenceMode _referenceMode;
        private float _worldDirection;

        public TurretAimState State => new TurretAimState(
            _currentYaw, _targetYaw, _currentPitch, _targetPitch,
            _referenceMode, _worldDirection);

        public TurretMechanism(
            TurretAimState state,
            float traverseDegreesPerSecond,
            float elevationDegreesPerSecond,
            float minPitch,
            float maxPitch)
        {
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

            _currentYaw = state.CurrentYaw;
            _targetYaw = state.TargetYaw;
            _currentPitch = Clamp(state.CurrentPitch, _minPitch, _maxPitch);
            _targetPitch = _currentPitch;
            _referenceMode = state.ReferenceMode;
            _worldDirection = state.WorldDirection;

            _lastHullYaw = NormalizeAngle(
                _worldDirection - _currentYaw);
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

            _targetYaw = _currentYaw;
            _worldDirection = NormalizeAngle(
                _lastHullYaw + _currentYaw);

            _referenceMode =
                _referenceMode ==
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

            if (_referenceMode ==
                TurretReferenceMode.WorldStabilized)
            {
                _worldDirection =
                    NormalizeAngle(
                        _worldDirection + yawStep);

                _targetYaw = NormalizeAngle(
                    _worldDirection - _lastHullYaw);
            }
            else
            {
                _targetYaw =
                    NormalizeAngle(
                        _targetYaw + yawStep);

                _worldDirection =
                    NormalizeAngle(
                        _lastHullYaw + _targetYaw);
            }

            _targetPitch = Clamp(
                _targetPitch +
                _elevationInput *
                _elevationDegreesPerSecond *
                deltaTime,
                _minPitch,
                _maxPitch);

            _currentYaw = MoveTowardsAngle(
                _currentYaw,
                _targetYaw,
                _traverseDegreesPerSecond * deltaTime);

            _currentPitch = MoveTowards(
                _currentPitch,
                _targetPitch,
                _elevationDegreesPerSecond * deltaTime);
        }

        public void ClearInput()
        {
            _traverseInput = 0f;
            _elevationInput = 0f;

            _targetYaw = _currentYaw;
            _targetPitch = _currentPitch;
            _worldDirection = NormalizeAngle(
                _lastHullYaw + _currentYaw);
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

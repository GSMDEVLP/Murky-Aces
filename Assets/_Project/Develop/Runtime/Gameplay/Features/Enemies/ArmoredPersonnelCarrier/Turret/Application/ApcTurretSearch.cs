using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Application
{
    public sealed class ApcTurretSearch
    {
        private readonly float _halfAngle;
        private readonly float _degreesPerSecond;
        private readonly Random _random;

        private float _targetAngle;

        public float Angle { get; private set; }

        public ApcTurretSearch(
            float searchAngle,
            float degreesPerSecond,
            Random random)
        {
            if (float.IsNaN(searchAngle) ||
                searchAngle <= 0f ||
                searchAngle > 360f)
            {
                throw new ArgumentOutOfRangeException(nameof(searchAngle));
            }

            if (float.IsNaN(degreesPerSecond) ||
                float.IsInfinity(degreesPerSecond) ||
                degreesPerSecond <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(degreesPerSecond));
            }

            _halfAngle = searchAngle * 0.5f;
            _degreesPerSecond = degreesPerSecond;
            _random = random ??
                throw new ArgumentNullException(nameof(random));

            ChooseNextAngle();
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            if (deltaTime <= 0f)
                return;

            float difference = _targetAngle - Angle;
            float step = _degreesPerSecond * deltaTime;

            if (Math.Abs(difference) <= step)
            {
                Angle = _targetAngle;
                ChooseNextAngle();
                return;
            }

            Angle += Math.Sign(difference) * step;
        }

        private void ChooseNextAngle()
        {
            _targetAngle =
                ((float)_random.NextDouble() * 2f - 1f) * _halfAngle;
        }
    }
}
using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Domain
{
    public sealed class TankFuelState
    {
        public float Capacity { get; }
        public float Amount { get; private set; }
        public float NormalizedAmount => Amount / Capacity;

        public TankFuelState(float capacity, float initialAmount)
        {
            if (float.IsNaN(capacity) ||
                float.IsInfinity(capacity) ||
                capacity <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (float.IsNaN(initialAmount) ||
                float.IsInfinity(initialAmount) ||
                initialAmount < 0f ||
                initialAmount > capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(initialAmount));
            }

            Capacity = capacity;
            Amount = initialAmount;
        }

        public float Add(float amount)
        {
            ValidateAmount(amount);

            float previous = Amount;
            Amount = Math.Min(Capacity, Amount + amount);

            return Amount - previous;
        }

        public float Consume(float amount)
        {
            ValidateAmount(amount);

            float previous = Amount;
            Amount = Math.Max(0f, Amount - amount);

            return previous - Amount;
        }

        private static void ValidateAmount(float amount)
        {
            if (float.IsNaN(amount) ||
                float.IsInfinity(amount) ||
                amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
        }
    }
}
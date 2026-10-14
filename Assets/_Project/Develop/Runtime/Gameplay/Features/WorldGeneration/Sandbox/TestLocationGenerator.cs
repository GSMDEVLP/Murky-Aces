using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    public sealed class TestLocationGenerator
    {
        public LocationLayout Generate(
            int seed,
            Vector2 center,
            Vector2 size,
            int pointCount)
        {
            if (size.x <= 0f || size.y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (pointCount < 2)
                throw new ArgumentOutOfRangeException(nameof(pointCount));

            var random = new System.Random(seed);
            var points = new Vector2[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                float progress = (float)i / (pointCount - 1);

                float x = center.x
                    + (progress - 0.5f) * size.x * 0.8f;

                bool isEndpoint = i == 0 || i == pointCount - 1;

                float zOffset = isEndpoint
                    ? 0f
                    : ((float)random.NextDouble() - 0.5f)
                        * size.y * 0.6f;

                points[i] = new Vector2(x, center.y + zOffset);
            }

            return new LocationLayout(center, size, points);
        }
    }
}
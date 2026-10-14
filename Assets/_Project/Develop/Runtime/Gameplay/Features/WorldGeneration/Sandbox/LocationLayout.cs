using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    public sealed class LocationLayout
    {
        public Vector2 Center { get; }
        public Vector2 Size { get; }
        public IReadOnlyList<Vector2> RoadPoints { get; }

        public LocationLayout(
            Vector2 center,
            Vector2 size,
            IEnumerable<Vector2> roadPoints)
        {
            if (roadPoints == null)
                throw new ArgumentNullException(nameof(roadPoints));

            Center = center;
            Size = size;

            // Собственная копия: исходный список можно менять
            // без изменения уже созданного плана.
            RoadPoints = new List<Vector2>(roadPoints).AsReadOnly();
        }
    }
}
using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public sealed class InteractionMenuState
    {
        public int Count { get; }
        public int SelectedIndex { get; private set; }
        public InteractionMenuState(int count)
        {
            if (count < 2) throw new ArgumentOutOfRangeException(nameof(count));
            Count = count;
        }
        public void Move(int direction)
        {
            if (direction != 0)
                SelectedIndex = (SelectedIndex + (direction > 0 ? 1 : Count - 1)) % Count;
        }
    }
}

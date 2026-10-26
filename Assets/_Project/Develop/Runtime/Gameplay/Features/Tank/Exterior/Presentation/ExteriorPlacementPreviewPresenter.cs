using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Presentation
{
    public sealed class ExteriorPlacementPreviewPresenter
    {
        private readonly PlayerInteraction _interaction;
        private readonly InteractionContext _context;
        private readonly ExteriorPlacementPreviewView _view;

        public ExteriorPlacementPreviewPresenter(PlayerInteraction interaction,
            InteractionContext context, ExteriorPlacementPreviewView view)
        {
            _interaction = interaction;
            _context = context;
            _view = view;
        }

        public void UpdateView()
        {
            if (!_interaction.IsGameplayEnabled || _interaction.IsChoosingAction ||
                !_interaction.HasFocusedTarget || !_interaction.FocusedTarget.IsActive ||
                _interaction.FocusedTarget is not IExteriorPlacementPreviewSource source ||
                !source.TryGetPlacementPreview(_context, out var prefab, out var anchor))
            {
                _view.Hide();
                return;
            }
            _view.TryShow(prefab, anchor);
        }
    }
}

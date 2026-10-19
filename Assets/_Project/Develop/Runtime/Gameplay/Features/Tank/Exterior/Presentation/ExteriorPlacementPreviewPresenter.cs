using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Presentation
{
    public sealed class ExteriorPlacementPreviewPresenter
    {
        private readonly PlayerInteraction _interaction;
        private readonly InteractionContext _context;
        private readonly ExteriorPlacementPreviewView _view;

        public ExteriorPlacementPreviewPresenter(
            PlayerInteraction interaction,
            InteractionContext context,
            ExteriorPlacementPreviewView view)
        {
            _interaction = interaction;
            _context = context;
            _view = view;
        }

        public void UpdateView()
        {
            if (!_interaction.IsGameplayEnabled ||
                !_interaction.HasFocusedTarget ||
                !(_interaction.FocusedTarget is ExteriorMountSlotAuthoring slot) ||
                slot == null ||
                !(_context.Actor is IPickupReceiver receiver))
            {
                _view.Hide();
                return;
            }

            IHoldableItem item = receiver.HeldItem;

            if (!(item is Component component) ||
                component == null ||
                !item.IsHeld ||
                !component.TryGetComponent(out ExteriorAttachmentAuthoring attachment) ||
                !slot.CanAccept(attachment))
            {
                _view.Hide();
                return;
            }

            _view.TryShow(
                attachment.Config.PreviewPrefab,
                slot.PlacementAnchor);
        }
    }
}
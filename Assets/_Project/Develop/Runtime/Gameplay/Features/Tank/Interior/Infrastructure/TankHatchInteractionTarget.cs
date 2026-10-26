using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Interior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class TankHatchInteractionTarget :
        InteractionTargetBehaviour, IExteriorPlacementPreviewSource
    {
        [SerializeField] private TankHatchInteractable _passage;
        [SerializeField] private ExteriorMountInstallInteractable _mount;

        public override void CollectActions(
            in InteractionContext context, List<IInteractionAction> actions)
        {
            if (!IsActive || _passage == null || !_passage.IsActive)
                return;

            IPickupReceiver receiver = context.Actor as IPickupReceiver;
            if (receiver != null && receiver.HeldItem is Component item &&
                item != null && receiver.HeldItem.IsHeld &&
                IsMountContext(item))
            {
                int firstMountAction = actions.Count;
                _mount.CollectActions(context, actions);
                for (int i = firstMountAction; i < actions.Count; i++)
                {
                    if (actions[i].GetInteractionInfo(context).IsAvailable)
                        return;
                }
                actions.RemoveRange(firstMountAction, actions.Count - firstMountAction);
            }

            _passage.CollectActions(context, actions);
            if (receiver != null && !receiver.IsOccupied && _mount != null)
                _mount.CollectActions(context, actions);
        }

        public bool TryGetPlacementPreview(in InteractionContext context,
            out GameObject previewPrefab, out Transform placementAnchor)
        {
            previewPrefab = null;
            placementAnchor = null;
            return IsActive && _mount != null && _mount.IsActive &&
                _mount.TryGetPlacementPreview(context, out previewPrefab, out placementAnchor);
        }

        private bool IsMountContext(Component item)
        {
            if (_mount == null || !_mount.IsActive || _mount.Slot == null)
                return false;
            if (item.TryGetComponent(out WeldingTool tool) && tool.isActiveAndEnabled)
                return true;
            return item.TryGetComponent(out ExteriorAttachmentAuthoring attachment) &&
                attachment.isActiveAndEnabled && attachment.Config != null &&
                attachment.Config.CompatibleMountType == _mount.Slot.State.MountType;
        }
    }
}

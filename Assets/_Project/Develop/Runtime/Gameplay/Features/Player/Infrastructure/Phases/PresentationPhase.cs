using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Phases
{
    public sealed class PresentationPhase : IPresentationTickable
    {
        private readonly InteractionHudPresenter _interactionHudPresenter;
        private readonly IPlayerGpsCamera _gpsCameraView;
        private readonly ExteriorPlacementPreviewPresenter _exteriorPreviewPresenter;

        public PresentationPhase(
            InteractionHudPresenter interactionHudPresenter,
            IPlayerGpsCamera gpsCameraView,
            ExteriorPlacementPreviewPresenter exteriorPreviewPresenter)
        {
            _interactionHudPresenter = interactionHudPresenter;
            _gpsCameraView = gpsCameraView;
            _exteriorPreviewPresenter = exteriorPreviewPresenter;
        }

        public void Tick(float deltaTime)
        {
            if (_gpsCameraView.IsAvailable)
                _gpsCameraView.Tick(deltaTime);

            _interactionHudPresenter.UpdateView();
            _exteriorPreviewPresenter.UpdateView();
        }
    }
}
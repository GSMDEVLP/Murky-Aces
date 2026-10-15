using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases
{
    public sealed class PresentationPhase : IPresentationTickable
    {
        private readonly InteractionHudPresenter _interactionHudPresenter;
        private readonly IPlayerGpsCamera _gpsCameraView;

        public PresentationPhase(
            InteractionHudPresenter interactionHudPresenter,
            IPlayerGpsCamera gpsCameraView)
        {
            _interactionHudPresenter = interactionHudPresenter;
            _gpsCameraView = gpsCameraView;
        }

        public void Tick(float deltaTime)
        {
            if (_gpsCameraView.IsAvailable)
                _gpsCameraView.Tick(deltaTime);

            _interactionHudPresenter.UpdateView();
        }
    }
}
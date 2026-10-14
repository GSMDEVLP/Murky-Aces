using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.GPS;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases
{
    public sealed class PresentationPhase : IPresentationTickable
    {
        private readonly InteractionHudPresenter _interactionHudPresenter;
        private readonly PlayerGpsCameraView _gpsCameraView;

        public PresentationPhase(
            InteractionHudPresenter interactionHudPresenter,
            PlayerGpsCameraView gpsCameraView)
        {
            _interactionHudPresenter = interactionHudPresenter;
            _gpsCameraView = gpsCameraView;
        }

        public void Tick(float deltaTime)
        {
            _gpsCameraView.Tick(deltaTime);
            _interactionHudPresenter.UpdateView();
        }
    }
}
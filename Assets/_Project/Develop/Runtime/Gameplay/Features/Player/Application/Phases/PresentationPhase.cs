using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases
{
    public sealed class PresentationPhase : IPresentationTickable
    {
        private readonly InteractionHudPresenter _interactionHudPresenter;

        public PresentationPhase(InteractionHudPresenter interactionHudPresenter)
        {
            _interactionHudPresenter = interactionHudPresenter;
        }

        public void Tick(float deltaTime)
        {
            _interactionHudPresenter.UpdateView();
        }
    }
}

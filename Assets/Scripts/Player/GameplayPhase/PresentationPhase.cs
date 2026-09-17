public sealed class PresentationPhase
{
    private readonly InteractionHudPresenter _interactionHudPresenter;

    public PresentationPhase(InteractionHudPresenter interactionHudPresenter)
    {
        _interactionHudPresenter = interactionHudPresenter;
    }

    public void Tick()
    {
        _interactionHudPresenter.UpdateView();
    }
}
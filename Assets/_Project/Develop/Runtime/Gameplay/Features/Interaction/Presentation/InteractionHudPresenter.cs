using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation
{
public sealed class InteractionHudPresenter
{
    private readonly PlayerInteraction _model;
    private readonly InteractionHudView _view;

    public InteractionHudPresenter(
        PlayerInteraction model,
        InteractionHudView view)
    {
        _model = model;
        _view = view;
    }

    public void UpdateView()
    {
        if (!_model.HasFocusedTarget ||
            !_model.FocusedInfo.IsAvailable)
        {
            _view.Hide();
            return;
        }

        InteractionInfo info =
            _model.FocusedInfo;

        if (info.Mode == InteractionMode.Press)
        {
            _view.ShowPress(info.Prompt);
            return;
        }

        _view.ShowHold(info.Prompt, CalculateHoldCompletion());
    }

    private float CalculateHoldCompletion()
    {
        if (!_model.IsHolding)
            return 0f;

        float requiredSeconds =
            _model.ActiveHoldRequiredSeconds;

        if (requiredSeconds <= 0f)
            return 0f;

        return _model.ActiveHoldElapsedSeconds /
               requiredSeconds;
    }
}
}

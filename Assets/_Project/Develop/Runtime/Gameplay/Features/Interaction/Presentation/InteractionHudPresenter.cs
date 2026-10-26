using System.Collections.Generic;
using System.Text;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation
{
    public sealed class InteractionHudPresenter
    {
        private readonly PlayerInteraction _model;
        private readonly InteractionHudView _view;
        private readonly StringBuilder _text = new StringBuilder(256);
        private IReadOnlyList<InteractionActionOption> _lastOptions;
        private int _lastSelection = -1;
        private string _selectionText;

        public InteractionHudPresenter(PlayerInteraction model, InteractionHudView view)
        {
            _model = model;
            _view = view;
        }

        public void UpdateView()
        {
            if (_model.IsChoosingAction)
            {
                UpdateSelectionText();
                _view.ShowSelection(_selectionText);
                return;
            }

            _lastOptions = null;
            _lastSelection = -1;
            if (!_model.HasFocusedTarget || !_model.FocusedInfo.IsAvailable)
            {
                _view.Hide();
                return;
            }

            InteractionInfo info = _model.FocusedInfo;
            if (info.Mode == InteractionMode.Press)
                _view.ShowPress(info.Prompt);
            else
                _view.ShowHold(info.Prompt, CalculateHoldCompletion());
        }

        private void UpdateSelectionText()
        {
            var options = _model.ActionOptions;
            int selected = _model.SelectedActionIndex;
            if (ReferenceEquals(options, _lastOptions) && selected == _lastSelection)
                return;

            _lastOptions = options;
            _lastSelection = selected;
            _text.Clear();
            _text.Append("<size=85%><color=#AAB7C4>CHOOSE ACTION</color></size>\n\n");
            for (int i = 0; i < options.Count; i++)
            {
                bool highlighted = i == selected;
                _text.Append(highlighted ? "<color=#F4C574>> " : "  ");
                _text.Append(options[i].Info.Prompt);
                if (options[i].Info.Mode == InteractionMode.Hold)
                    _text.Append("  <size=75%>[hold E]</size>");
                if (highlighted) _text.Append("</color>");
                _text.Append('\n');
            }
            _text.Append("\n<size=65%><color=#AAB7C4>W/S or arrows: select\nE: confirm   Esc: cancel</color></size>");
            _selectionText = _text.ToString();
        }

        private float CalculateHoldCompletion()
        {
            float required = _model.ActiveHoldRequiredSeconds;
            return _model.IsHolding && required > 0f
                ? _model.ActiveHoldElapsedSeconds / required : 0f;
        }
    }
}

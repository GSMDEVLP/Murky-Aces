using TMPro;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation
{
    public sealed class InteractionHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private GameObject _holdProgressRoot;
        [SerializeField] private UnityEngine.UI.Image _holdFill;
        [SerializeField] private GameObject _selectionRoot;
        [SerializeField] private TMP_Text _selectionText;

        private void Awake() => Hide();

        public void Hide()
        {
            _contentRoot.SetActive(false);
            if (_selectionRoot != null) _selectionRoot.SetActive(false);
        }

        public void ShowPress(InteractionPromts prompt)
        {
            ShowContent();
            _promptText.text = "[E] " + (prompt == InteractionPromts.Choose ? "Actions" : prompt.ToString());
            _holdProgressRoot.SetActive(false);
        }

        public void ShowHold(InteractionPromts prompt, float completedFraction)
        {
            ShowContent();
            _promptText.text = "[Hold E] " + prompt;
            _holdProgressRoot.SetActive(true);
            _holdFill.fillAmount = Mathf.Clamp01(completedFraction);
        }

        public void ShowSelection(string text)
        {
            _contentRoot.SetActive(false);
            if (_selectionRoot == null || _selectionText == null)
                return;
            _selectionRoot.SetActive(true);
            if (_selectionText.text != text) _selectionText.text = text;
        }

        private void ShowContent()
        {
            if (_selectionRoot != null) _selectionRoot.SetActive(false);
            _contentRoot.SetActive(true);
        }
    }
}

using TMPro;
using UnityEngine;

public sealed class InteractionHudView : MonoBehaviour
{
    [SerializeField] private GameObject _contentRoot;

    [SerializeField] private TMP_Text _promptText;

    [SerializeField] private GameObject _holdProgressRoot;

    [SerializeField] private UnityEngine.UI.Image _holdFill;

    private void Awake()
    {
        Hide();
    }

    public void Hide()
    {
        _contentRoot.SetActive(false);
    }

    public void ShowPress(InteractionPromts prompt)
    {
        _contentRoot.SetActive(true);
        _promptText.text = prompt.ToString();
        _holdProgressRoot.SetActive(false);
    }

    public void ShowHold(InteractionPromts prompt, float completedFraction)
    {
        _contentRoot.SetActive(true);
        _promptText.text = prompt.ToString();
        _holdProgressRoot.SetActive(true);
        _holdFill.fillAmount =
            Mathf.Clamp01(completedFraction);
    }
}
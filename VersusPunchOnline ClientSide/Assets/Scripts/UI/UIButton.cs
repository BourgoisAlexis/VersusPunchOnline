using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public UnityEvent onClick;

    private Image _image;
    private TextMeshProUGUI _tmproContent;
    private float _fadeDuration = 0.15f;

    private void Awake() {
        _image = GetComponent<Image>();
        _tmproContent = GetComponentInChildren<TextMeshProUGUI>();

        if (_image != null)
            _image.color = Color.white;

        if (_tmproContent != null)
            _tmproContent.color = Color.black;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _image?.DOColor(Color.black, _fadeDuration);
        _tmproContent?.DOColor(Color.white, _fadeDuration);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _image?.DOColor(Color.white, _fadeDuration);
        _tmproContent?.DOColor(Color.black, _fadeDuration);
    }

    public void OnPointerClick(PointerEventData eventData) {
        onClick?.Invoke();
    }
}

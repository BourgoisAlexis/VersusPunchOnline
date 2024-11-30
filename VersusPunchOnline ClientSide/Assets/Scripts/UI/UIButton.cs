using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    #region Variables
    public UnityEvent onClick;

    private Image _image;
    private TextMeshProUGUI _tmproContent;
    private float _fadeDuration = 0.15f;

    private Color _light => Color.white;
    private Color _dark => Color.black;
    #endregion


    private void Awake() {
        _image = GetComponent<Image>();
        _tmproContent = GetComponentInChildren<TextMeshProUGUI>();
        ReInit();
    }

    private void OnEnable() {
        ReInit();
    }

    private void ReInit() {
        if (_image != null)
            _image.color = _light;

        if (_tmproContent != null)
            _tmproContent.color = _dark;
    }


    public void OnPointerEnter(PointerEventData eventData) {
        _image?.DOColor(_dark, _fadeDuration);
        _tmproContent?.DOColor(_light, _fadeDuration);
        transform.DOScale(1.1f, _fadeDuration);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _image?.DOColor(_light, _fadeDuration);
        _tmproContent?.DOColor(_dark, _fadeDuration);
        transform.DOScale(1f, _fadeDuration);
    }

    public void OnPointerClick(PointerEventData eventData) {
        //GlobalManager.Instance.SFXManager.PlayAudio(19);
        onClick?.Invoke();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    #region Variables
    public UnityEvent OnClick;

    private Image _image;
    private Image _icon;
    private TextMeshProUGUI _tmproContent;
    private float _fadeDuration = 0.1f;
    private Ease _ease = Ease.InSine;
    private Vector3 _initialEuler;

    private Color _light => Color.white;
    private Color _dark => Color.black;
    #endregion


    private void Awake() {
        Image[] images = GetComponentsInChildren<Image>();
        _image = images[0];
        _icon = images.Length > 1 ? images[1] : null;
        _tmproContent = GetComponentInChildren<TextMeshProUGUI>(true);
        _initialEuler = transform.localEulerAngles;
        ReInit();
    }

    private void OnEnable() {
        ReInit();
    }

    private void ReInit() {
        if (_image != null)
            _image.color = _light;

        if (_icon != null)
            _icon.color = _dark;

        if (_tmproContent != null)
            _tmproContent.color = _dark;

        transform.localEulerAngles = _initialEuler;
    }


    public void OnPointerEnter(PointerEventData eventData) {
        _image?.DOColor(_dark, _fadeDuration);
        _icon?.DOColor(_light, _fadeDuration);
        _tmproContent?.DOColor(_light, _fadeDuration);
        transform.DOScale(1.1f, _fadeDuration).SetEase(_ease);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _image?.DOColor(_light, _fadeDuration);
        _icon?.DOColor(_dark, _fadeDuration);
        _tmproContent?.DOColor(_dark, _fadeDuration);
        transform.DOScale(1f, _fadeDuration).SetEase(_ease);
    }

    public async void OnPointerClick(PointerEventData eventData) {
        await transform.DOLocalRotate(_initialEuler + Vector3.forward * 5, _fadeDuration / 2).SetEase(_ease).AsyncWaitForCompletion();
        transform.DOLocalRotate(_initialEuler, _fadeDuration / 2).SetEase(_ease);
        //GlobalManager.Instance.SFXManager.PlayAudio(19);
        OnClick?.Invoke();
    }
}

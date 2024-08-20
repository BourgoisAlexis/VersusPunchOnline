using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class UITransitionManager : MonoBehaviour {
    [SerializeField] private Transform _background;
    [SerializeField] private Transform[] _stripes;

    [SerializeField] private float[] _stripesPos;
    private CanvasGroup _canvasGroup;


    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();
        int length = _stripes.Length;
        _stripesPos = new float[length];

        for (int i = 0; i < length; i++)
            _stripesPos[i] = _stripes[i].localPosition.x;

        Hide(true);
    }

    public async Task Show() {
        int delay = 50;

        _background.gameObject.SetActive(true);
        await Task.Delay(delay);
        _background.gameObject.SetActive(false);
        await Task.Delay(delay);
        _background.gameObject.SetActive(true);
        await Task.Delay(delay);

        for (int i = _stripes.Length - 1; i > -1; i--) {
            AnimStripe(_stripes[i], _stripesPos[i]);
            await Task.Delay(delay);
        }
    }

    public async Task Hide(bool instant = false) {
        if (instant) {
            _background.gameObject.SetActive(false);
            foreach (Transform t in _stripes)
                t.localPosition = Vector3.left * 2000;

            return;
        }

        int delay = 50;

        for (int i = 0; i < _stripes.Length; i++) {
            AnimStripe(_stripes[i], -2000);
            await Task.Delay(delay);
        }

        _background.gameObject.SetActive(false);
    }

    private void AnimStripe(Transform t, float destination) {
        t.DOLocalMoveX(destination, 0.08f).SetEase(Ease.InOutSine);
    }
}

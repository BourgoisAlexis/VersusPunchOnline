using System.Collections.Generic;
using UnityEngine.UI;

public class BonusChoiceView : UIView {
    protected override void Init(params object[] parameters) {
        if (_buttons == null || _buttons[0].buttons.Count < 2) {
            Utils.LogError(this, "No buttons setup");
            return;
        }

        GetCurrentButton().OnPointerEnter(null);

        List<UIButton> buttons = _buttons[0].buttons;

        for (int i = 0; i < 2; i++)
            SetupButton(buttons[i], parameters[i] as Bonus, (int)parameters[2]);

        PlayerController ctrl = parameters[2] as PlayerController;
        ctrl.PlugView(this);
    }

    public void SetupButton(UIButton button, Bonus bonus, int playerIndex) {
        button.GetComponent<Image>().sprite = bonus.Sprite;
        GameplaySceneManager manager = GlobalManager.Instance.SceneManager as GameplaySceneManager;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            manager.ChooseBonus(playerIndex, bonus.Id);
        });
    }
}

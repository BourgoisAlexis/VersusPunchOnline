using System.Collections.Generic;

public class PlayerBonus {
    private List<Bonus> _bonusList;

    public PlayerBonus(IEnumerable<string> bonusIds = null) {
        _bonusList = new List<Bonus>();
        if (bonusIds != null)
            AddBonus(bonusIds);
    }

    public void AddBonus(IEnumerable<string> bonusIds) {
        foreach (string s in bonusIds)
            _bonusList.Add(GlobalManager.Instance.BonusDataBase.GetBonusByID(s));
    }

    public void RemoveBonus() {

    }

    public void Clear() {
        _bonusList.Clear();
    }

    public void UseBonus(int index) {
        if (index < 0 || index >= _bonusList.Count) {
            Utils.LogError(this, "UseBonus", $"You try to use bonus at index {index}");
            return;
        }

        _bonusList[index].Use();
    }
}

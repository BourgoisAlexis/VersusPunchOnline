using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusDataBase", menuName = "BonusDataBase")]
public class BonusDataBase : ScriptableObject {
    [SerializeField] private List<Bonus> _bonusList;
    [SerializeField] private System.Random _random;

    public void Init() {
        _random = new System.Random(AppConst.RANDOM_SEED);
    }

    public Bonus GetBonusByID(string id) {
        return _bonusList.Find(x => x.Id == id);
    }

    public Bonus GetRandomBonus() {
        return _bonusList[_random.Next(_bonusList.Count)];
    }
}

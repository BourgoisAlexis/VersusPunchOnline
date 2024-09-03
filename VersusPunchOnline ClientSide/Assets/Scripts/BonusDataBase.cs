using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusDataBase", menuName = "BonusDataBase")]
public class BonusDataBase : ScriptableObject {
    [SerializeField] private List<Bonus> bonus;
    [SerializeField] private System.Random _random;

    public void Init() {
        _random = new System.Random(AppConst.randomSeed);
    }

    public Bonus GetBonusByID(string id) {
        return bonus.Find(x => x.id == id);
    }

    public Bonus GetRandomBonus() {
        return bonus[_random.Next(bonus.Count)];
    }
}

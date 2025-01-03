using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusDataBase", menuName = "BonusDataBase")]
public class BonusDataBase : ScriptableObject {
    [SerializeField][SerializeReference] private List<Bonus> _bonusList;
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



#if UNITY_EDITOR
    public void AddBonus(Bonus bonus) {
        _bonusList.Add(bonus);
    }
}

[CustomEditor(typeof(BonusDataBase))]
public class BonusDataBaseEditor : Editor {
    private BonusType _bonusType = BonusType.Default;

    public override void OnInspectorGUI() {
        GUILayout.BeginVertical();
        BonusDataBase data = (BonusDataBase)target;

        if (GUILayout.Button("Add"))
            AddBonus(data);

        _bonusType = (BonusType)EditorGUILayout.EnumPopup("Bonus Type", _bonusType);

        GUILayout.EndVertical();

        base.OnInspectorGUI();
    }

    private void AddBonus(BonusDataBase data) {
        Bonus b = null;

        switch (_bonusType) {
            case BonusType.Default:
                b = new Bonus();
                break;

            case BonusType.Projectile:
                Debug.Log(_bonusType);
                b = new Projectile();
                break;

            case BonusType.Shield:
                b = new Bonus();
                break;
        }

        b.Id = _bonusType.ToString();
        b.Description = $"This is a {b.Id}";
        data.AddBonus(b);
    }
}
#endif
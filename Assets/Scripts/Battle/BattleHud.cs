using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour {

	[SerializeField] Text nameText;
  [SerializeField] Text levelText;
  [SerializeField] Text StatusMessage;
  [SerializeField] Text StatusText;
  [SerializeField] HpBar hpBar;
  [SerializeField] GameObject expBar;

  [SerializeField] Color psnColor;
  [SerializeField] Color bldColor;
  [SerializeField] Color brnColor;
  [SerializeField] Color parColor;
  [SerializeField] Color frzColor;
  [SerializeField] Color slpColor;

  Monster _monster;

  Dictionary<ConditionID, Color> statusColors;

  public void SetData(Monster monster)
  {
    _monster = monster;

    nameText.text = monster.Base.Name;
    SetLevel();
    hpBar.SetHP( (float) monster.HP / monster.MaxHp);
    SetExp();

    statusColors = new Dictionary<ConditionID, Color>(){
      {ConditionID.psn, psnColor},
      {ConditionID.bld, bldColor},
      {ConditionID.brn, brnColor},
      {ConditionID.par, parColor},
      {ConditionID.frz, frzColor},
      {ConditionID.slp, slpColor}
    };

    SetStatusText();
    _monster.OnStatusChanged += SetStatusText;
  }

  void SetStatusText(){
    if (_monster.Status == null){
      StatusText.text = "";
      StatusMessage.text = "";
    }else{
      // "aportuguesando" a parada
      if(_monster.Status.Id.ToString() == "psn")
        StatusText.text = "VNN";
      else if(_monster.Status.Id.ToString() == "bld")
        StatusText.text = "SNG";
      else if(_monster.Status.Id.ToString() == "brn")
        StatusText.text = "QMD";
      else if(_monster.Status.Id.ToString() == "par")
        StatusText.text = "PAR";
      else if(_monster.Status.Id.ToString() == "frz")
        StatusText.text = "CGL";
      else
        StatusText.text = "DMD";
      
      // StatusText.text = _monster.Status.Id.ToString().ToUpper();
      StatusMessage.text = "Estado";
      StatusText.color = statusColors[_monster.Status.Id];
    }
  }

  public void SetLevel(){
    levelText.text = "Nvl " + _monster.Level;
  }

  public void SetExp(){
    if(expBar == null) return;

    float normalizedExp = GetNormalizedExp();
    expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
  }

  public IEnumerator SetExpSmooth(bool reset = false){
    if(expBar == null) yield break;

    if(reset)
      expBar.transform.localScale = new Vector3(0, 1, 1);
    float normalizedExp = GetNormalizedExp();
    yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
  }

  float GetNormalizedExp(){
    int currLevelExp = _monster.Base.GetExpForLevel(_monster.Level);
    int nextLevelExp = _monster.Base.GetExpForLevel(_monster.Level + 1);

    float normalizedExp = (float)(_monster.Exp - currLevelExp) / (nextLevelExp - currLevelExp);

    return Mathf.Clamp01(normalizedExp);
  }

  public IEnumerator UpdateHP()
  {
    if (_monster.HpChanged){
      yield return hpBar.SetHPSmooth( (float) _monster.HP / _monster.MaxHp);
      _monster.HpChanged = false;
    }
  }
}

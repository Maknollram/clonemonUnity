using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour {

	[SerializeField] Text nameText;
  [SerializeField] Text levelText;
  [SerializeField] HpBar hpBar;
  [SerializeField] Color highlightedColor;

  Monster _monster;

  // Monster data when start battle
  public void SetData(Monster monster){
    _monster = monster;

    nameText.text = monster.Base.Name;
    levelText.text = "Nvl " + monster.Level;
    hpBar.SetHP( (float) monster.HP / monster.MaxHp);
  }

  // change color of selected monster in party screen
  public void SetSelected(bool selected){
    if (selected)
      nameText.color = highlightedColor;
    else
      nameText.color = Color.black;
  }
}

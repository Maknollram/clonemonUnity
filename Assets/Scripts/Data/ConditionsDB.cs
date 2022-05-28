using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

public class ConditionsDB {
  public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition> () {
    {
      ConditionID.psn,
      new Condition() {
        Name = "Envenenado",
        StartMessage = "está envenenado!",
        OnAfterTurn = (Monster monster) => {
          monster.UpdateHP(monster.MaxHp / 8);
          monster.StatusChanges.Enqueue($"{monster.Base.Name} poderá morrer pelo veneno.");
        }
      }
    },
    {
      ConditionID.brn,
      new Condition() {
        Name = "Queimadura",
        StartMessage = "está queimando em chamas!",
        OnAfterTurn = (Monster monster) => {
          if ((monster.MaxHp / 16) <= 1)
            monster.UpdateHP((monster.MaxHp / 16) + 1);
          else
            monster.UpdateHP(monster.MaxHp / 16);

          monster.StatusChanges.Enqueue($"Se {monster.Base.Name} continuar queimando, morrerá.");
        }
      }
    }
  };
}

public enum ConditionID {
  none,
  [Description("Sangramento")] // like badly poison
  bld,
  [Description("Queimadura")] // don't affect water monsters
  brn,
  [Description("Confusão")]
  con,
  [Description("Enfurecido")] // increase atk, spAtk and spd but decrease def and spDef
  enr,
  [Description("Congelamento")]
  frz,
  [Description("Amedrontado")] // don't act for 2 turns and decrease def and spDef
  frg,
  [Description("Paralisia")] // can act or cannot in 5 turns
  par,
  [Description("Envenenamento")]
  psn,
  [Description("Sono")] // can't act for 3 turns and take more damage
  slp,
  [Description("Molhado")] // decrease speed and accuracy and take more damage from water/ice/electric moves
  wet
}

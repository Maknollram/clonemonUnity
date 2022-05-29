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
      ConditionID.bld,
      new Condition() {
        Name = "Sangramento",
        StartMessage = "está sangrando!",
        OnAfterTurn = (Monster monster) => {
          monster.UpdateHP((monster.MaxHp / 16) +  (monster.BleedTime + 1));
          monster.BleedTime++;
          monster.StatusChanges.Enqueue($"{monster.Base.Name} morrerá rápido se não se curar do sangramento.");
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
    },
    {
      ConditionID.par,
      new Condition() {
        Name = "Paralisia",
        StartMessage = "está paralizado!",
        OnBeforeMove = (Monster monster) => {
          if (Random.Range(1, 4) == 1){ // three turns
            monster.StatusChanges.Enqueue($"{monster.Base.Name} continua paralizado e não pode se mover.");
            return false;
          }
          return true;
        }
      }
    },
    {
      ConditionID.frz,
      new Condition() {
        Name = "Congelamento",
        StartMessage = "está congelado!",
        OnBeforeMove = (Monster monster) => {
          if (Random.Range(1, 4) == 1){ // three turns
            monster.CureStatus();
            monster.StatusChanges.Enqueue($"{monster.Base.Name} quebrou o gelo.");
            return true;
          }

          if ((monster.MaxHp / 16) <= 1)
            monster.UpdateHP((monster.MaxHp / 16) + 1);
          else
            monster.UpdateHP(monster.MaxHp / 16);

          monster.StatusChanges.Enqueue($"Se {monster.Base.Name} continuar congelado poderá morrer de frio.");
          return false;
        }
      }
    },
    {
      ConditionID.slp,
      new Condition() {
        Name = "Sono",
        StartMessage = "está dormindo agora!",
        OnStart = (Monster monster) => {
          monster.StatusTime = Random.Range(1, 4);
        },
        OnBeforeMove = (Monster monster) => {
          if (monster.StatusTime <= 0){ // three turns
            monster.CureStatus();
            monster.StatusChanges.Enqueue($"{monster.Base.Name} acordou.");
            return true;
          }

          monster.StatusTime--;
          monster.StatusChanges.Enqueue($"{monster.Base.Name} está em sono profundo, mas talvez acorde em breve.");
          return false;
        }
      }
    }
  };
}

public enum ConditionID {
  none,
  [Description("Sangramento")] // like badly poison
  bld,
  [Description("Queimadura")] // don't affect water and fire monsters
  brn,
  [Description("Confusão")]
  con,
  [Description("Enfurecido")] // increase atk, spAtk and spd but decrease def and spDef
  enr,
  [Description("Congelamento")] // like paralize, but cure after 3 turns and take damage from the status
  frz,
  [Description("Amedrontado")] // don't act for 2 turns and decrease def and spDef
  frg,
  [Description("Paralisia")] // can act or cannot in 3 turns
  par,
  [Description("Envenenamento")]
  psn,
  [Description("Sono")] // can't act for 3 turns, take more damage and awake if take damage
  slp,
  [Description("Molhado")] // decrease speed and accuracy and take more damage from water/ice/electric moves
  wet
}

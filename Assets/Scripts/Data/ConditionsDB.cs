using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

public class ConditionsDB {
  public static void Init(){
    foreach (var cond in Conditions){
      var conditionId = cond.Key;
      var condition = cond.Value;

      condition.Id = conditionId;
    }
  }

  public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition> () {
    // status ailments
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
    },
    // volatile status conditions
    {
      ConditionID.confusion,
      new Condition() {
        Name = "Desorientado",
        StartMessage = "está vendo estrelas!",
        OnStart = (Monster monster) => {
          monster.VolatileStatusTime = Random.Range(1, 4);
        },
        OnBeforeMove = (Monster monster) => {
          if (monster.VolatileStatusTime <= 0){ // three turns
            monster.CureVolatileStatus();
            monster.StatusChanges.Enqueue($"{monster.Base.Name} voltou ao normal.");
            return true;
          }

          monster.VolatileStatusTime--;

          // chance to use move (50%)
          if(Random.Range(1, 3) == 1)
            return true;

          // if don't pass in the above verification hurt himself
          monster.StatusChanges.Enqueue($"{monster.Base.Name} está desorientado.");
          monster.UpdateHP(monster.MaxHp / 12);
          monster.StatusChanges.Enqueue($"{monster.Base.Name} está tão desorientado que atacou a si mesmo.");
          return false;
        }
      }
    },
    {
      ConditionID.spikes,
      new Condition() {
        Name = "Espinhos",
        StartMessage = "está rodeado por espinhos!",
        OnStart = (Monster monster) => {
          monster.VolatileStatusTime = Random.Range(1, 4);
        },
        OnAfterTurn = (Monster monster) => {
          if (monster.VolatileStatusTime <= 0){ // three turns
            monster.CureVolatileStatus();
            monster.StatusChanges.Enqueue($"Os espinhos se quebraram.");
          }

          monster.VolatileStatusTime--;

          // hurt the enemy
          monster.StatusChanges.Enqueue($"Os espinhos estão furando {monster.Base.Name}.");
          monster.UpdateHP(monster.MaxHp / 10);
          monster.StatusChanges.Enqueue($"Remova os espinhos de {monster.Base.Name} ou espere que eles se quebrem.");
        }
      }
    }
  };

  public static float GetStatusBonus(Condition condition){
    if(condition == null)
      return 1f;
    else if(condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
      return 2f;
    else if(condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.bld || condition.Id == ConditionID.brn)
      return 1.5f;

    return 1f;
  }
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
  wet,
  [Description("Desorientado")] // confused for 3 turns and hurt himself
  confusion,
  [Description("Espinhos")] // for 3 turns the enemy is hurted by spykes
  spikes
}

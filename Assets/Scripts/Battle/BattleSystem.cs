using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour {

	[SerializeField] BattleUnit playerUnit;
  [SerializeField] BattleUnit enemyUnit;
  [SerializeField] BattleHud playerHud;
  [SerializeField] BattleHud enemyHud;
  [SerializeField] BattleDialogBox dialogBox;

  BattleState state;
  int currentAction;
  int currentMove;

  private void Start()
  {
    StartCoroutine(SetupBattle());
  }

  public IEnumerator SetupBattle()
  {
    playerUnit.Setup();
    enemyUnit.Setup();
    playerHud.SetData(playerUnit.Monster);
    enemyHud.SetData(enemyUnit.Monster);

    dialogBox.SetMoveNames(playerUnit.Monster.Moves);

    yield return dialogBox.TypeDialog("Um " + enemyUnit.Monster.Base.Name + " selvagem apareceu.");

    PlayerAction();
  }

  void PlayerAction()
  {
    state = BattleState.PlayerAction;
    StartCoroutine(dialogBox.TypeDialog("Escolha uma ação"));
    dialogBox.EnableActionSelector(true);
  }

  void PlayerMove()
  {
    state = BattleState.PlayerMove;
    dialogBox.EnableActionSelector(false);
    dialogBox.EnableDialogText(false);
    dialogBox.EnableMoveSelector(true);
  }

  IEnumerator PerformPlayerMove()
  {
    state = BattleState.Busy;

    var move = playerUnit.Monster.Moves[currentMove];
    yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " usou " + move.Base.Name + ".");

    var damageDetails = enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
    yield return enemyHud.UpdateHP();
    yield return ShowDamageDetails(enemyUnit, damageDetails);

    if (damageDetails.Fainted)
    {
      yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " morreu!");
    }
    else
    {
      StartCoroutine(EnemyMove());
    }
  }

  IEnumerator EnemyMove()
  {
    state = BattleState.EnemyMove;

    var move = enemyUnit.Monster.GetRandomMove();

    yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " usou " + move.Base.Name + ".");

    var damageDetails = playerUnit.Monster.TakeDamage(move, playerUnit.Monster);
    yield return playerHud.UpdateHP();
    yield return ShowDamageDetails(playerUnit, damageDetails);

    if (damageDetails.Fainted)
    {
      yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " morreu!");
    }
    else
    {
      PlayerAction();
    }
  }

  IEnumerator ShowDamageDetails(BattleUnit battleUnit, DamageDetails damageDetails)
  {
    Debug.Log(damageDetails.TypeEffectiveness);
    if (damageDetails.Critical > 1f)
      yield return dialogBox.TypeDialog(battleUnit.Monster.Base.Name + " recebeu um ataque crítico!");

    if (damageDetails.TypeEffectiveness <= 0f)
      yield return dialogBox.TypeDialog("Este ataque não afetou " + battleUnit.Monster.Base.Name + "!");
    else if (damageDetails.TypeEffectiveness > 1f && damageDetails.TypeEffectiveness <= 1.25f)
      yield return dialogBox.TypeDialog(battleUnit.Monster.Base.Name + " recebeu o ataque de um Deus!");
    else if (damageDetails.TypeEffectiveness > 1.25f)
      yield return dialogBox.TypeDialog(battleUnit.Monster.Base.Name + " recebeu um ataque muito forte!");
    else if (damageDetails.TypeEffectiveness < 1f)
      yield return dialogBox.TypeDialog(battleUnit.Monster.Base.Name + " recebeu um ataque muito fraco!");
  }

  private void Update()
  {
    if (state == BattleState.PlayerAction)
    {
      HandleActionSelection();
    }
    else if (state == BattleState.PlayerMove)
    {
      HandleMoveSelection();
    }
  }

  void HandleActionSelection()
  {
    if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      if(currentAction < 1)
        ++currentAction;
    }else if(Input.GetKeyDown(KeyCode.UpArrow))
    {
      if(currentAction > 0)
        --currentAction;
    }

    dialogBox.UpdateActionSelection(currentAction);

    if (Input.GetKeyDown(KeyCode.Alpha0))
    {
        if(currentAction == 0)
        {
          // Let's fight
          PlayerMove();
        }
        else if(currentAction == 1)
        {
          // Run like a crazy bitch MAN

        }
    }
  }

  void HandleMoveSelection()
  {
    if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      if(currentMove < playerUnit.Monster.Moves.Count - 1)
        ++currentMove;
    }
    else if(Input.GetKeyDown(KeyCode.LeftArrow))
    {
      if(currentMove > 0)
        --currentMove;
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      if(currentMove < playerUnit.Monster.Moves.Count - 2)
        currentMove += 2;
    }else if(Input.GetKeyDown(KeyCode.UpArrow))
    {
      if(currentMove > 1)
        currentMove -= 2;
    }

    dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

    if (Input.GetKeyDown(KeyCode.Alpha0))
    {
      dialogBox.EnableMoveSelector(false);
      dialogBox.EnableDialogText(true);
      StartCoroutine(PerformPlayerMove());
    }
  }
}

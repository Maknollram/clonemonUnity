﻿using System.Collections;
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
    yield return new WaitForSeconds(1f);

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
    yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " used " + move.Base.Name);

    yield return new WaitForSeconds(1f);

    bool isFainted = enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
    yield return enemyHud.UpdateHP();

    if (isFainted)
    {
      yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " fainted");
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

    yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " used " + move.Base.Name);

    yield return new WaitForSeconds(1f);

    bool isFainted = playerUnit.Monster.TakeDamage(move, playerUnit.Monster);
    yield return playerHud.UpdateHP();

    if (isFainted)
    {
      yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " fainted");
    }
    else
    {
      PlayerAction();
    }
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour {

  private const string joystick1 = "joystick 1 button ";
	private const int CROSS = 0;
	private const int CIRCLE = 1;
	private const int SQUARE = 2;
	private const int TRIANGLE = 3;
	private const int L = 4;
	private const int R = 5;
	private const int SELECT = 6;
	private const int START = 7;
	private const int UP = 8;
	private const int RIGHT = 9;
	private const int DOWN = 10;
	private const int LEFT = 11;

	[SerializeField] BattleUnit playerUnit;
  [SerializeField] BattleUnit enemyUnit;
  [SerializeField] BattleHud playerHud;
  [SerializeField] BattleHud enemyHud;
  [SerializeField] BattleDialogBox dialogBox;

  public event Action<bool> OnBattleOver;

  BattleState state;
  int currentAction;
  int currentMove;

  MonsterParty playerParty;
  Monster wildMonster;

  public void StartBattle(MonsterParty playerParty, Monster wildMonster)
  {
    this.playerParty = playerParty;
    this.wildMonster = wildMonster;
    StartCoroutine(SetupBattle());
  }

  public IEnumerator SetupBattle()
  {
    playerUnit.Setup(playerParty.GetHealthyMonster());
    enemyUnit.Setup(wildMonster);
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
    move.SP--; // mudar para incrementar no final
    yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " usou " + move.Base.Name + ".");

    playerUnit.PlayAttackAnimation();
    yield return new WaitForSeconds(1f);

    var damageDetails = enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
    
    if(damageDetails.TypeEffectiveness > 0f)
      enemyUnit.PlayHitAnimation();

    yield return enemyHud.UpdateHP();
    yield return ShowDamageDetails(enemyUnit, damageDetails);

    if (damageDetails.Fainted)
    {
      yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " morreu!");
      enemyUnit.PlayFaintAnimation();
      playerUnit.PlayVictoryAnimation();

      yield return new WaitForSeconds(2f);
      OnBattleOver(true);
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
    move.SP--; // mudar para incrementar no final
    yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " usou " + move.Base.Name + ".");

    enemyUnit.PlayAttackAnimation();
    yield return new WaitForSeconds(1f);

    var damageDetails = playerUnit.Monster.TakeDamage(move, playerUnit.Monster);
    
    if(damageDetails.TypeEffectiveness > 0f)
      playerUnit.PlayHitAnimation();

    yield return playerHud.UpdateHP();
    yield return ShowDamageDetails(playerUnit, damageDetails);

    if (damageDetails.Fainted)
    {
      yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " morreu!");
      playerUnit.PlayFaintAnimation();
      enemyUnit.PlayVictoryAnimation();

      yield return new WaitForSeconds(2f);

      var nextMonster = playerParty.GetHealthyMonster();
      if (nextMonster != null)
      {
        playerUnit.Setup(nextMonster);
        playerHud.SetData(nextMonster);

        dialogBox.SetMoveNames(nextMonster.Moves);

        yield return dialogBox.TypeDialog("Pega ele " + nextMonster.Base.Name + ".");

        PlayerAction();
      }
      else{
        OnBattleOver(false);
      }
    }
    else
    {
      PlayerAction();
    }
  }

  IEnumerator ShowDamageDetails(BattleUnit battleUnit, DamageDetails damageDetails)
  {
    if (damageDetails.Critical > 1f && damageDetails.TypeEffectiveness > 0f)
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

  public void HandleUpdate()
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
    if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
    {
      if(currentAction < 1)
        ++currentAction;
    }else if(Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow))
    {
      if(currentAction > 0)
        --currentAction;
    }

    dialogBox.UpdateActionSelection(currentAction);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad0))
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
    if (Input.GetKeyDown(joystick1 + RIGHT) || Input.GetKeyDown(KeyCode.RightArrow))
    {
      if(currentMove < playerUnit.Monster.Moves.Count - 1)
        ++currentMove;
    }
    else if(Input.GetKeyDown(joystick1 + LEFT) || Input.GetKeyDown(KeyCode.LeftArrow))
    {
      if(currentMove > 0)
        --currentMove;
    }
    else if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
    {
      if(currentMove < playerUnit.Monster.Moves.Count - 2)
        currentMove += 2;
    }else if(Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow))
    {
      if(currentMove > 1)
        currentMove -= 2;
    }

    dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad0))
    {
      dialogBox.EnableMoveSelector(false);
      dialogBox.EnableDialogText(true);
      StartCoroutine(PerformPlayerMove());
    }
  }
}

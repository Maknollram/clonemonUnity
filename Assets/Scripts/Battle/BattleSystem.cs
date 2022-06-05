using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}

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
  [SerializeField] BattleDialogBox dialogBox;
  [SerializeField] PartyScreen partyScreen;

  public event Action<bool> OnBattleOver;

  BattleState state;
  int currentAction;
  int currentMove;
  int currentMember;

  MonsterParty playerParty;
  Monster wildMonster;

  public void StartBattle(MonsterParty playerParty, Monster wildMonster){
    this.playerParty = playerParty;
    this.wildMonster = wildMonster;
    StartCoroutine(SetupBattle());
  }

  public IEnumerator SetupBattle(){
    playerUnit.Setup(playerParty.GetHealthyMonster());
    enemyUnit.Setup(wildMonster);

    partyScreen.Init();

    dialogBox.SetMoveNames(playerUnit.Monster.Moves);

    yield return dialogBox.TypeDialog($"Um {enemyUnit.Monster.Base.Name} apareceu.");

    ChooseFirstTurn();
  }

  void ChooseFirstTurn(){
    if (playerUnit.Monster.Speed >= enemyUnit.Monster.Speed)
      ActionSelection();
    else
      StartCoroutine(EnemyMove());
  }

  void BattleOver(bool finished){
    state = BattleState.BattleOver;
    playerParty.Monsters.ForEach(m => m.OnBattleOver()); // reset monsters stats and status
    OnBattleOver(finished);
  }

  void ActionSelection(){
    state = BattleState.ActionSelection;
    StartCoroutine(dialogBox.TypeDialog("Escolha uma ação"));
    dialogBox.EnableActionSelector(true);
  }

  void OpenPartyScreen(){
    state = BattleState.PartyScreen;
    partyScreen.SetPartyData(playerParty.Monsters);
    partyScreen.gameObject.SetActive(true);
  }

  void MoveSelection(){
    state = BattleState.MoveSelection;
    dialogBox.EnableActionSelector(false);
    dialogBox.EnableDialogText(false);
    dialogBox.EnableMoveSelector(true);
  }

  IEnumerator PlayerMove(){
    state = BattleState.PerformMove;

    var move = playerUnit.Monster.Moves[currentMove];
    yield return RunMove(playerUnit, enemyUnit, move);

    if (state == BattleState.PerformMove)
      StartCoroutine(EnemyMove());
  }

  IEnumerator EnemyMove(){
    state = BattleState.PerformMove;

    var move = enemyUnit.Monster.GetRandomMove();
    yield return RunMove(enemyUnit, playerUnit, move);

    if (state == BattleState.PerformMove)
      ActionSelection();
  }

  IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move){

    // to verify status ailments like paralize, sleep and freeze for example
    bool canRunMove = sourceUnit.Monster.OnBeforeMove();
    if(!canRunMove){
      yield return ShowStatusChanges(sourceUnit.Monster);
      yield return sourceUnit.Hud.UpdateHP(); // if the status do damage, like freeze, confusion, etc
      yield break;
    }
    yield return ShowStatusChanges(sourceUnit.Monster);
    // ========================================================================

    move.SP--; // mudar para incrementar no final
    yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} usou {move.Base.Name}.");

    sourceUnit.PlayAttackAnimation(); // I changed a little
    yield return new WaitForSeconds(1f);
    if(CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster)){
      if (move.Base.Category == MoveCategory.Status){
        yield return RunMoveEffects(sourceUnit, targetUnit, move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
      }else{
        var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
    
        if(damageDetails.TypeEffectiveness > 0f && move.Base.Category != MoveCategory.Status)
          targetUnit.PlayHitAnimation();

        yield return targetUnit.Hud.UpdateHP();
        yield return ShowDamageDetails(targetUnit, damageDetails);
      }

      if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Monster.HP > 0){
        foreach (var secondary in move.Base.Secondaries)
        {
          var rnd = UnityEngine.Random.Range(1, 101);

          if (rnd <= secondary.Chance)
            yield return RunMoveEffects(sourceUnit, targetUnit, secondary, sourceUnit.Monster, targetUnit.Monster, secondary.Target);
        }
      }

      if (targetUnit.Monster.HP <= 0){
        yield return dialogBox.TypeDialog($"{ targetUnit.Monster.Base.Name} morreu!");
        targetUnit.PlayFaintAnimation();
        sourceUnit.PlayVictoryAnimation();
        yield return new WaitForSeconds(2f);
        
        CheckForBattleOver(targetUnit);
      }
    }else{
      yield return dialogBox.TypeDialog($"{ sourceUnit.Monster.Base.Name} errou o alvo!");
    }

    sourceUnit.Monster.OnAfterTurn();
    yield return ShowStatusChanges(sourceUnit.Monster);
    yield return sourceUnit.Hud.UpdateHP();

    if (sourceUnit.Monster.HP <= 0){
      yield return dialogBox.TypeDialog($"{ sourceUnit.Monster.Base.Name} morreu!");
      sourceUnit.PlayFaintAnimation();
      targetUnit.PlayVictoryAnimation();
      yield return new WaitForSeconds(2f);
      
      CheckForBattleOver(sourceUnit);
    }
  }

  // correcao, alterar para ficar com somente souceUnit e targetUnit
  IEnumerator RunMoveEffects(BattleUnit sourceUnit, BattleUnit targetUnit, MoveEffects effects, Monster source, Monster target, MoveTarget moveTarget){
    //stats boosts
    if (effects.Boosts != null){
      if (moveTarget == MoveTarget.Self){
        source.ApplyBoosts(effects.Boosts);
        sourceUnit.PlayStatBoostAnimation();
      }
      else{
        target.ApplyBoosts(effects.Boosts);
        targetUnit.PlayStatBoostAnimation();
      }
    }

    // status ailments (ailments, weather, etc)
    if (effects.Status != ConditionID.none){
      target.SetStatus(effects.Status);
      if (moveTarget == MoveTarget.Self){
        sourceUnit.PlayStatusAilmentsAnimation();
      }
      else{
        targetUnit.PlayStatusAilmentsAnimation();
      }
    }

    // volatile status conditions (confusion, etc)
    if (effects.VolatileStatus != ConditionID.none){
      target.SetVolatileStatus(effects.VolatileStatus);
      if (moveTarget == MoveTarget.Self){
        sourceUnit.PlayStatusAilmentsAnimation();
      }
      else{
        targetUnit.PlayStatusAilmentsAnimation();
      }
    }

    yield return ShowStatusChanges(source);
    yield return ShowStatusChanges(target);
  }

  bool CheckIfMoveHits(Move move, Monster source, Monster target){
    if(move.Base.AlwaysHits)
      return true;

    float moveAccuracy = move.Base.Accuracy;

    int accuracy = source.StatBoosts[Stat.Accuracy];
    int evasion = target.StatBoosts[Stat.Evasion];

    var boostValues = new float[] {1f, 4f/3f, 5f/3f, 2f, 7f/3f, 8f/3f, 3f}; 

    if(accuracy > 0)
      moveAccuracy *= boostValues[accuracy];
    else
      moveAccuracy /= boostValues[-accuracy];

    if(evasion > 0)
      moveAccuracy /= boostValues[evasion];
    else
      moveAccuracy *= boostValues[-evasion];

    return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
  }

  IEnumerator ShowStatusChanges(Monster monster){
    while (monster.StatusChanges.Count > 0){
      var message = monster.StatusChanges.Dequeue();
      yield return dialogBox.TypeDialog(message);
    }
  }

  void CheckForBattleOver(BattleUnit faintedUnit){
    if (faintedUnit.IsPlayerUnit){
      var nextMonster = playerParty.GetHealthyMonster();
      
      if (nextMonster != null)
        OpenPartyScreen();
      else
        BattleOver(false);
    }else
      BattleOver(true);
  }

  IEnumerator ShowDamageDetails(BattleUnit battleUnit, DamageDetails damageDetails){
    if (damageDetails.Critical > 1f && damageDetails.TypeEffectiveness > 0f)
      yield return dialogBox.TypeDialog($"{battleUnit.Monster.Base.Name} recebeu um ataque crítico!");

    if (damageDetails.TypeEffectiveness <= 0f)
      yield return dialogBox.TypeDialog($"Este ataque não afetou {battleUnit.Monster.Base.Name}!");
    else if (damageDetails.TypeEffectiveness > 1f && damageDetails.TypeEffectiveness <= 1.25f)
      yield return dialogBox.TypeDialog($"{battleUnit.Monster.Base.Name} recebeu o ataque de um Deus!");
    else if (damageDetails.TypeEffectiveness > 1.25f)
      yield return dialogBox.TypeDialog($"{battleUnit.Monster.Base.Name} recebeu um ataque muito forte!");
    else if (damageDetails.TypeEffectiveness < 1f)
      yield return dialogBox.TypeDialog($"{battleUnit.Monster.Base.Name} recebeu um ataque muito fraco!");
  }

  public void HandleUpdate(){
    if (state == BattleState.ActionSelection){
      HandleActionSelection();
    }else if (state == BattleState.MoveSelection){
      HandleMoveSelection();
    }else if (state == BattleState.PartyScreen){
      HandlePartySelection();
    }
  }

  void HandleActionSelection(){
    // somente cima/baixo
    // if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
    // {
    //   if(currentAction < 1)
    //     ++currentAction;
    // }else if(Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow))
    // {
    //   if(currentAction > 0)
    //     --currentAction;
    // }

    if (Input.GetKeyDown(joystick1 + RIGHT) || Input.GetKeyDown(KeyCode.RightArrow))
      ++currentAction;
    else if(Input.GetKeyDown(joystick1 + LEFT) || Input.GetKeyDown(KeyCode.LeftArrow))
      --currentAction;
    else if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
      currentAction += 2;
    else if(Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow))
      currentAction -= 2;

    currentAction = Mathf.Clamp(currentAction, 0, 3);

    dialogBox.UpdateActionSelection(currentAction);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2)){
        if(currentAction == 0){
          // Let's fight
          MoveSelection();
        }else if(currentAction == 1){
          // Slap an item

        }else if(currentAction == 2){
          // Take your monster Comrade
          OpenPartyScreen();
        }else if(currentAction == 3){
          // Run like a crazy bitch MAN

        }
    }
  }

  void HandleMoveSelection(){
    if (Input.GetKeyDown(joystick1 + RIGHT) || Input.GetKeyDown(KeyCode.RightArrow))
      ++currentMove;
    else if(Input.GetKeyDown(joystick1 + LEFT) || Input.GetKeyDown(KeyCode.LeftArrow))
      --currentMove;
    else if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
      currentMove += 2;
    else if(Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow))
      currentMove -= 2;

    currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Monster.Moves.Count - 1);

    dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2)){
      dialogBox.EnableMoveSelector(false);
      dialogBox.EnableDialogText(true);
      StartCoroutine(PlayerMove());
    }else if(Input.GetKeyDown(joystick1 + CIRCLE) || Input.GetKeyDown(KeyCode.Keypad3)){
      dialogBox.EnableMoveSelector(false);
      dialogBox.EnableDialogText(true);
      ActionSelection();
    }
  }

  void HandlePartySelection(){
    if (Input.GetKeyDown(joystick1 + RIGHT) || Input.GetKeyDown(KeyCode.RightArrow)){
      ++currentMember;
      partyScreen.SetMessageText("");
    }else if (Input.GetKeyDown(joystick1 + LEFT) || Input.GetKeyDown(KeyCode.LeftArrow)){
      --currentMember;
      partyScreen.SetMessageText("");
    }else if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow)){
      currentMember += 2;
      partyScreen.SetMessageText("");
    }else if (Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow)){
      currentMember -= 2;
      partyScreen.SetMessageText("");
    }

    currentMember = Mathf.Clamp(currentMember, 0, playerParty.Monsters.Count - 1);

    partyScreen.UpdateMemberSelection(currentMember);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2)){
      var selectedMember = playerParty.Monsters[currentMember];

      if (selectedMember.HP <= 0){
        partyScreen.SetMessageText("Monstro morto não pode batalhar!");
        return;
      }

      if (selectedMember == playerUnit.Monster){
        partyScreen.SetMessageText("Este monstro está em campo!");
        return;
      }

      partyScreen.gameObject.SetActive(false);
      state = BattleState.Busy;
      StartCoroutine(SwitchMonster(selectedMember));
    }else if (Input.GetKeyDown(joystick1 + CIRCLE) || Input.GetKeyDown(KeyCode.Keypad3)){
      partyScreen.gameObject.SetActive(false);
      ActionSelection();
    }
  }

  IEnumerator SwitchMonster(Monster newMonster){
    bool currentMonsterFainted = true;

    if (playerUnit.Monster.HP > 0){
      currentMonsterFainted = false;
      yield return dialogBox.TypeDialog($"Retorne {playerUnit.Monster.Base.Name}.");
      playerUnit.PlayFaintAnimation();
      yield return new WaitForSeconds(1.5f);
    }

    playerUnit.Setup(newMonster);
    dialogBox.SetMoveNames(newMonster.Moves);

    yield return dialogBox.TypeDialog($"Arrebenta {newMonster.Base.Name}.");

    if (currentMonsterFainted)
      ChooseFirstTurn();
    else
      StartCoroutine(EnemyMove());
  }
}
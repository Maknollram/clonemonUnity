using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver}

public enum BattleAction { Move, SwitchMonster, UseItem, Run }

public class BattleSystem : MonoBehaviour {

  // psvita controller
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
  // ======================================================

  [SerializeField] BattleUnit playerUnit;
  [SerializeField] BattleUnit enemyUnit;
  [SerializeField] BattleDialogBox dialogBox;
  [SerializeField] PartyScreen partyScreen;
  [SerializeField] Image playerImage;
  [SerializeField] Image trainerImage;
  [SerializeField] GameObject monsterballSprite;

  public event Action<bool> OnBattleOver;

  BattleState state;
  BattleState? prevState;
  int currentAction;
  int currentMove;
  int currentMember;
  bool aboutToUseChoice = true;

  MonsterParty playerParty;
  MonsterParty trainerParty;
  Monster wildMonster;

  bool isTrainerBattle = false;

  PlayerController player;
  TrainerController trainer;

  public void StartBattle(MonsterParty playerParty, Monster wildMonster){
    this.playerParty = playerParty;
    this.wildMonster = wildMonster;
    player = playerParty.GetComponent<PlayerController>();

    StartCoroutine(SetupBattle());
  }

  public void StartTrainerBattle(MonsterParty playerParty, MonsterParty trainerParty){
    this.playerParty = playerParty;
    this.trainerParty = trainerParty;

    isTrainerBattle = true;
    player = playerParty.GetComponent<PlayerController>();
    trainer = trainerParty.GetComponent<TrainerController>();

    StartCoroutine(SetupBattle());
  }

  public IEnumerator SetupBattle(){
    // disable monsters hud
    playerUnit.Clear();
    enemyUnit.Clear();

    // verification if is wild monster battle or trainer battle
    if(!isTrainerBattle){
      playerUnit.Setup(playerParty.GetHealthyMonster());
      enemyUnit.Setup(wildMonster);
    
      dialogBox.SetMoveNames(playerUnit.Monster.Moves);

      yield return dialogBox.TypeDialog($"Um {enemyUnit.Monster.Base.Name} apareceu.");
    }else{
      // hide monsters sprites
      playerUnit.gameObject.SetActive(false);
      enemyUnit.gameObject.SetActive(false);

      // show trainer and player sprites
      playerImage.gameObject.SetActive(true);
      trainerImage.gameObject.SetActive(true);
      playerImage.sprite = player.Sprite;
      trainerImage.sprite = trainer.Sprite;

      yield return dialogBox.TypeDialog($"{trainer.Name} desafiou {player.Name} para uma batalha de monstros.");

      // trainer first monster
      trainerImage.gameObject.SetActive(false);
      enemyUnit.gameObject.SetActive(true);
      var enemyMonster = trainerParty.GetHealthyMonster();
      enemyUnit.Setup(enemyMonster);
      yield return dialogBox.TypeDialog($"{trainer.Name} liberou {enemyMonster.Base.Name}!");

      // player first monster
      playerImage.gameObject.SetActive(false);
      playerUnit.gameObject.SetActive(true);
      var playerMonster = playerParty.GetHealthyMonster();
      playerUnit.Setup(playerMonster);
      yield return dialogBox.TypeDialog($"{RandomBattleText(true)} {playerMonster.Base.Name}");
      dialogBox.SetMoveNames(playerUnit.Monster.Moves);
    }

    partyScreen.Init();

    ActionSelection();
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
    state = BattleState.RunningTurn;
    dialogBox.EnableActionSelector(false);
    dialogBox.EnableDialogText(false);
    dialogBox.EnableMoveSelector(true);
  }

  IEnumerator AboutToUse(Monster newMonster){
    state = BattleState.Busy;
    yield return dialogBox.TypeDialog($"{trainer.Name} liberará {newMonster.Base.Name}. Deseja trocar de monstro?");

    state = BattleState.AboutToUse;
    dialogBox.EnableChoiceBox(true);
  }

  IEnumerator RunTurns(BattleAction playerAction){
    state = BattleState.RunningTurn;

    if(playerAction == BattleAction.Move){
      playerUnit.Monster.CurrentMove = playerUnit.Monster.Moves[currentMove];
      enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove();

      // moves priority
      int playerMovePriority = playerUnit.Monster.CurrentMove.Base.Priority;
      int enemyMovePriority = enemyUnit.Monster.CurrentMove.Base.Priority;

      // verify who goes first
      bool playerGoesFirst = true;
      if(enemyMovePriority > playerMovePriority)
        playerGoesFirst = false;
      else if(enemyMovePriority == playerMovePriority)
        playerGoesFirst = playerUnit.Monster.Speed >= enemyUnit.Monster.Speed;

      var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
      var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

      var secondMonster = secondUnit.Monster;

      // first turn
      yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
      yield return RunAfterTurn(firstUnit, secondUnit);
      if(state == BattleState.BattleOver) yield break;

      if(secondMonster.HP > 0){
        // second turn
        yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
        yield return RunAfterTurn(secondUnit, firstUnit);
        if(state == BattleState.BattleOver) yield break;
      }
    }else{
      if(playerAction == BattleAction.SwitchMonster){
        var selectedMonster = playerParty.Monsters[currentMember];
        state = BattleState.Busy;
        yield return SwitchMonster(selectedMonster);
      } else if (playerAction == BattleAction.UseItem){
        dialogBox.EnableActionSelector(false);
        yield return ThrowMonsterball();
      }

      // enemy turn
      var enemyMove = enemyUnit.Monster.GetRandomMove();
      yield return RunMove(enemyUnit, playerUnit, enemyMove);
      yield return RunAfterTurn(enemyUnit, playerUnit);
      if(state == BattleState.BattleOver) yield break;
    }

    if(state != BattleState.BattleOver)
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

  IEnumerator RunAfterTurn(BattleUnit sourceUnit, BattleUnit targetUnit){
    if(state == BattleState.BattleOver) yield break;
    yield return new WaitUntil(() => state == BattleState.RunningTurn);

    // statuses like brn and psn do damage after turn
    sourceUnit.Monster.OnAfterTurn();
    yield return ShowStatusChanges(sourceUnit.Monster);
    yield return sourceUnit.Hud.UpdateHP();

    if (sourceUnit.Monster.HP <= 0){
      yield return dialogBox.TypeDialog($"{ sourceUnit.Monster.Base.Name} morreu!");
      sourceUnit.PlayFaintAnimation();
      targetUnit.PlayVictoryAnimation();
      yield return new WaitForSeconds(2f);
      
      CheckForBattleOver(sourceUnit);
      yield return new WaitUntil(() => state == BattleState.RunningTurn);
    }
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
    }else{
      if(!isTrainerBattle){
        BattleOver(true);
      }else{
        var nextMonster = trainerParty.GetHealthyMonster();
        if(nextMonster != null)
          StartCoroutine(AboutToUse(nextMonster));
        else
          BattleOver(true);
      }
    }
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
    }else if (state == BattleState.RunningTurn){
      HandleMoveSelection();
    }else if (state == BattleState.PartyScreen){
      HandlePartySelection();
    }else if (state == BattleState.AboutToUse){
      HandleAboutToUse();
    }

    // send monsterball with a button
    if(Input.GetKeyDown(joystick1 + SQUARE) || Input.GetKeyDown(KeyCode.Keypad1))
      StartCoroutine(RunTurns(BattleAction.UseItem));
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
          StartCoroutine(RunTurns(BattleAction.UseItem));
        }else if(currentAction == 2){
          // Take your monster comrade
          prevState = state;
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

    // on select a move
    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2)){
      var move = playerUnit.Monster.Moves[currentMove];
      if(move.SP == 0) return;

      dialogBox.EnableMoveSelector(false);
      dialogBox.EnableDialogText(true);
      StartCoroutine(RunTurns(BattleAction.Move));
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

      if(prevState == BattleState.ActionSelection){
        prevState = null;
        StartCoroutine(RunTurns(BattleAction.SwitchMonster));
      }else{
        state = BattleState.Busy;
        StartCoroutine(SwitchMonster(selectedMember));
      }
    }else if (Input.GetKeyDown(joystick1 + CIRCLE) || Input.GetKeyDown(KeyCode.Keypad3)){
      if (playerUnit.Monster.HP <= 0){
        partyScreen.SetMessageText("Você deve escolher um monstro para continuar a batalhar.");
        return;
      }

      partyScreen.gameObject.SetActive(false);

      if (prevState == BattleState.AboutToUse){
        prevState = null;
        StartCoroutine(SendNextTrainerMonster());
      }else
        ActionSelection();
    }
  }

  void HandleAboutToUse(){
    if (Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
      aboutToUseChoice = !aboutToUseChoice;

    dialogBox.UpdateChoiceBox(aboutToUseChoice);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2)){
      dialogBox.EnableChoiceBox(false);
      if(aboutToUseChoice == true){
        // yes option
        prevState = BattleState.AboutToUse;
        OpenPartyScreen();
      }else{
        // no option
        StartCoroutine(SendNextTrainerMonster());
      }
    }else if (Input.GetKeyDown(joystick1 + CIRCLE) || Input.GetKeyDown(KeyCode.Keypad3)){
      dialogBox.EnableChoiceBox(false);
      StartCoroutine(SendNextTrainerMonster());
    }
  }

  IEnumerator SwitchMonster(Monster newMonster){
    if (playerUnit.Monster.HP > 0){
      // var randomBackText = new List<string> { "Retorne", "Volte", "Regresse", "Venha", "Já fez o bastante" };
      // int randomBackIndex = UnityEngine.Random.Range(0, randomBackText.Count);
      yield return dialogBox.TypeDialog($"{RandomBattleText(false)} {playerUnit.Monster.Base.Name}.");
      playerUnit.PlayFaintAnimation();
      yield return new WaitForSeconds(1.5f);
    }

    playerUnit.Setup(newMonster);
    dialogBox.SetMoveNames(newMonster.Moves);

    // random text on init battle
    // var randomInitialText = new List<string> { "Pega ele", "Arrebenta", "Mostre para que veio", "Não vacile", "Vamos nessa" };
    // int randomInitialIndex = UnityEngine.Random.Range(0, randomInitialText.Count);
    yield return dialogBox.TypeDialog($"{RandomBattleText(true)} {newMonster.Base.Name}.");

    if (prevState == null){
      state = BattleState.RunningTurn;
    }else if (prevState == BattleState.AboutToUse){
      prevState = null;
      StartCoroutine(SendNextTrainerMonster());
    }
  }

  IEnumerator SendNextTrainerMonster(){
    state = BattleState.Busy;

    var nextMonster = trainerParty.GetHealthyMonster();
    enemyUnit.Setup(nextMonster);
    yield return dialogBox.TypeDialog($"{trainer.Name} liberou {nextMonster.Base.Name}!");

    state = BattleState.RunningTurn;
  }

  IEnumerator ThrowMonsterball(){
    state = BattleState.Busy;

    if(isTrainerBattle){
      yield return dialogBox.TypeDialog($"Monstros de outros colecionadores não podem se juntar à sua coleção!");
      state = BattleState.RunningTurn;
      yield break;
    }

    var monsterballObj = Instantiate(monsterballSprite, playerUnit.transform.position - new Vector3(2,0), Quaternion.identity);

    yield return dialogBox.TypeDialog($"{player.Name} lançou um capturador de monstro!");
    var monsterball = monsterballObj.GetComponent<SpriteRenderer>();

    // animations
    yield return monsterball.transform.DOJump(enemyUnit.transform.position + new Vector3(0,2), 2f, 1, 1f).WaitForCompletion();
    yield return enemyUnit.PlayCaptureAnimation();
    yield return monsterball.transform.DOMoveY(enemyUnit.transform.position.y -1.3f, 0.5f).WaitForCompletion();

    int shakeCount = TryToCatchMonster(enemyUnit.Monster);

    // shake monsterball 3 times
    for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
    {
      yield return new WaitForSeconds(0.5f);
      // like original
      // yield return monsterball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
      yield return monsterball.transform.DOScale(new Vector3(1.5f, 1.5f, 1f), 0.5f);
      yield return monsterball.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
    }

    if(shakeCount == 4){
      // monster caught
      yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} foi armazenado com sucesso!");
      yield return monsterball.DOFade(0, 1.5f).WaitForCompletion();

      playerParty.AddMonster(enemyUnit.Monster);
      yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} se juntou ao seu grupo!");

      Destroy(monsterball);
      BattleOver(true);
    }else{
      // monster released
      yield return new WaitForSeconds(1f);
      monsterball.DOFade(0, 0.2f);
      yield return enemyUnit.PlayBreakOutAnimation();

      if (shakeCount < 2)
        yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} se soltou!");
      else
        yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} por pouco não foi armazenado!");

      Destroy(monsterball);
      state = BattleState.RunningTurn;
    }
  }

  int TryToCatchMonster(Monster monster){
    // calculation based on original formulas gen 3/4
    float a = (3 * monster.MaxHp - 2 * monster.HP) * monster.Base.CatchRate * ConditionsDB.GetStatusBonus(monster.Status) / (3 * monster.MaxHp);

    if (a >= 255)
      return 4;

    float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

    int shakeCount = 0;
    while (shakeCount < 4){
      if(UnityEngine.Random.Range(0, 65535) >= b)
        break;

      ++shakeCount;
    }

    return shakeCount;
  }

  string RandomBattleText(bool send){
    var randomTextArray = new List<string>();
    if(send)
      randomTextArray = new List<string> { "Pega ele", "Arrebenta", "Mostre para que veio", "Não vacile", "Vamos nessa" };
    else
      randomTextArray = new List<string> { "Retorne", "Volte", "Regresse", "Venha", "Já fez o bastante" };
    
    // random text on battles
    int randomInitialIndex = UnityEngine.Random.Range(0, randomTextArray.Count);
    return randomTextArray[randomInitialIndex];
  }
}
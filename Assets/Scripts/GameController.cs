using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
  FreeRoam,
  Battle,
  Dialog,
  Cutscene,
  Paused
}

public class GameController : MonoBehaviour {
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

  [SerializeField] PlayerController playerController;
  [SerializeField] BattleSystem battleSystem;
  [SerializeField] Camera worldCamera;

  GameState state;
  GameState stateBeforePause;

  public SceneDetails CurrentScene { get; private set; }
  public SceneDetails PrevScene { get; private set; }

  public static GameController Instance { get; private set; }

  private void Awake(){
    Instance = this;
    ConditionsDB.Init();
  }

  private void Start(){
    battleSystem.OnBattleOver += EndBattle;

    DialogManager.Instance.OnShowDialog += () => {
      state = GameState.Dialog;
    };

    DialogManager.Instance.OnCloseDialog += () => {
      if(state == GameState.Dialog)
        state = GameState.FreeRoam;
    };
  }

  public void PauseGame(bool pause){
    if(pause){
      stateBeforePause = state;
      state = GameState.Paused;
    }else
      state = stateBeforePause;
  }

  public void StartBattle(){
    state = GameState.Battle;
    battleSystem.gameObject.SetActive(true);
    worldCamera.gameObject.SetActive(false);

    var playerParty = playerController.GetComponent<MonsterParty>();
    var wildMonster = CurrentScene.GetComponent<MapArea>().GetRandomWildMonster();

    var wildMonsterCopy = new Monster(wildMonster.Base, wildMonster.Level);

    battleSystem.StartBattle(playerParty, wildMonsterCopy);
  }

  TrainerController trainer;

  public void StartTrainerBattle(TrainerController trainer){
    state = GameState.Battle;
    battleSystem.gameObject.SetActive(true);
    worldCamera.gameObject.SetActive(false);

    this.trainer = trainer;
    var playerParty = playerController.GetComponent<MonsterParty>();
    var trainerParty = trainer.GetComponent<MonsterParty>();

    battleSystem.StartTrainerBattle(playerParty, trainerParty);
  }

  public void OnEnterTrainersView(TrainerController trainer){
    state = GameState.Cutscene;
    StartCoroutine(trainer.TriggerTrainerBatte(playerController));
  }

  void EndBattle(bool won){
    if(trainer != null && won == true){
      trainer.BattleLost();
      trainer = null;
    }

    state = GameState.FreeRoam;
    battleSystem.gameObject.SetActive(false);
    worldCamera.gameObject.SetActive(true);
  }

	private void Update(){
		if (state == GameState.FreeRoam){
      playerController.HandleUpdate();

      // save game via buttons only in the map
      if(Input.GetKeyDown(joystick1 + L) || Input.GetKeyDown(KeyCode.Keypad8))
        SavingSystem.i.Save("saveSlot1");

      // load game via buttons only in the map
      if(Input.GetKeyDown(joystick1 + R) || Input.GetKeyDown(KeyCode.Keypad9)){
        SavingSystem.i.Load("saveSlot1");
        Debug.Log("L");
      }

    }else if (state == GameState.Battle){
      battleSystem.HandleUpdate();
    }else if (state == GameState.Dialog){
      DialogManager.Instance.HandleUpdate();
    }
	}

  public void SetCurrentScene(SceneDetails currScene){
    PrevScene = CurrentScene;
    CurrentScene = currScene;
  }
}

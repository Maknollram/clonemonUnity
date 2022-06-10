using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable {
  [SerializeField] Dialog dialog;
  [SerializeField] List<Vector2> movementPattern;
  [SerializeField] float timeBetweenPattern;

  NPCState state;
  float idleTimer = 0f;

  int currentPattern = 0;

  Character character;

  private void Awake(){
    character = GetComponent<Character>();
  }

  public void Interact(){
    if(state == NPCState.Idle)
      StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    
    // npc run from you
    // StartCoroutine(character.Move(new Vector2(-2, 0)));
  }

  private void Update(){
    // npc stop if walking and player interact with them
    if(DialogManager.Instance.IsShowing) return;

    if(state == NPCState.Idle){
      idleTimer += Time.deltaTime;
      if(idleTimer > timeBetweenPattern){
        idleTimer = 0f;
        if(movementPattern.Count > 0)
          StartCoroutine(Walk());
      }
    }

    character.HandleUpdate();
  }

  IEnumerator Walk(){
    state = NPCState.Walking;

    yield return character.Move(movementPattern[currentPattern]);
    currentPattern = (currentPattern + 1) % movementPattern.Count;

    state = NPCState.Idle;
  }
}

public enum NPCState { Idle, Walking }

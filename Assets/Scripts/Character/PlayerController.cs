using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

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

  [SerializeField] string name;
  [SerializeField] Sprite sprite;

  const float offsetY = 0.3f;

  public event Action OnEncountered;
  public event Action<Collider2D> OnEnterTrainersView;

  private Vector2 input;

  private Character character;

  private void Awake(){
    character = GetComponent<Character>();
  }

  public void HandleUpdate(){
    if (!character.IsMoving){
      input.x = Input.GetAxisRaw("Horizontal");
      input.y = Input.GetAxisRaw("Vertical");

      // remove diagonal movement
      // if (input.x != 0) input.y = 0;

      if (input != Vector2.zero){
        StartCoroutine( character.Move(input, OnMoveOver) );
      }
    }

    character.HandleUpdate();

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2))
      Interact();
  }

  void Interact(){
    var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
    var interactPos = transform.position + facingDir;

    var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
    if (collider != null){
      collider.GetComponent<Interactable>()?.Interact(transform);
    }
  }

  private void OnMoveOver(){
    CheckForEncounters();
    CheckIfInTtrainersView();
  }

  private void CheckForEncounters(){
    if (Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.GrassLayer) != null){
      if (UnityEngine.Random.Range(1, 101) <= 10){
        character.Animator.IsMoving = false;
        OnEncountered();
      }
    }
  }

  private void CheckIfInTtrainersView(){
    var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.FovLayer);
    if (collider != null){
      character.Animator.IsMoving = false;
      OnEnterTrainersView?.Invoke(collider);
    }
  }

  public string Name { get => name; }

  public Sprite Sprite { get => sprite; }
}

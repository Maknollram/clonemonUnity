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

	public float moveSpeed;
  public LayerMask solidObjectsLayer;
  public LayerMask interactableLayer;
  public LayerMask grassLayer;

  public event Action OnEncountered;

  private bool isMoving;
  private Vector2 input;

  private Animator animator;

  private void Awake()
  {
    animator = GetComponent<Animator>();
  }

  public void HandleUpdate()
  {
    if (!isMoving)
    {
      input.x = Input.GetAxisRaw("Horizontal");
      input.y = Input.GetAxisRaw("Vertical");

      // remove diagonal movement
      // if (input.x != 0) input.y = 0;

      if (input != Vector2.zero)
      {
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);

        var targetPos = transform.position;
        targetPos.x += input.x;
        targetPos.y += input.y;

        if (IsWalkable(targetPos))
          StartCoroutine(Move(targetPos));
      }
    }

    animator.SetBool("isMoving", isMoving);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2))
      Interact();
  }

  void Interact(){
    var facingDir = new Vector3(animator.GetFloat("MoveX"), animator.GetFloat("MoveY"));
    var interactPos = transform.position + facingDir;

    var collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
    if (collider != null){
      collider.GetComponent<Interactable>()?.Interact();
    }
  }

  IEnumerator Move(Vector3 targetPos)
  {
    isMoving = true;

    while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
    {
      transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
      yield return null;
    }
    transform.position = targetPos;

    isMoving = false;

    CheckForEncounters();
  }

  private bool IsWalkable(Vector3 targetPos)
  {
    if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
    {
      return false;
    }

    return true;
  }

  private void CheckForEncounters()
  {
    if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
    {
      if (UnityEngine.Random.Range(1, 101) <= 10)
      {
        animator.SetBool("isMoving", false);
        OnEncountered();
      }
    }
  }
}

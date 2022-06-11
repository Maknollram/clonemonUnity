using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour {
  [SerializeField] Dialog dialog;
  [SerializeField] GameObject exclamation;
  [SerializeField] GameObject fov;

  Character character;

  private void Awake(){
    character = GetComponent<Character>();
  }

  private void Start(){
    SetFovRotation(character.Animator.DefaultDirection);
  }

  public IEnumerator TriggerTrainerBatte(PlayerController player){
    // show exclamation icon
    exclamation.SetActive(true);
    yield return new WaitForSeconds(0.5f);
    exclamation.SetActive(false);

    // walk towards the player
    var diff = player.transform.position - transform.position;
    var moveVec = diff - diff.normalized;
    moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

    yield return character.Move(moveVec);

    // show dialog and start battle
    StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
      Debug.Log("Batalhando freneticamente");
    }));
  }

  public void SetFovRotation(FacingDirection dir){
    float angle = 0f; // facing down as default
    if(dir == FacingDirection.UpRight)
      angle = 135f;
    else if(dir == FacingDirection.UpLeft)
      angle = 225f;
    else if(dir == FacingDirection.DownRight)
      angle = 415f;
    else if(dir == FacingDirection.DownLeft)
      angle = 315f;
    else if(dir == FacingDirection.Right)
      angle = 90f;
    else if(dir == FacingDirection.Left)
      angle = 270f;
    else if(dir == FacingDirection.Up)
      angle = 180f;

    fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
  }
}

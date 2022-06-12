using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable {
  [SerializeField] string name;
  [SerializeField] Sprite sprite;
  [SerializeField] Dialog dialog;
  [SerializeField] Dialog dialogAfterBattle;
  [SerializeField] GameObject exclamation;
  [SerializeField] GameObject fov;

  // state
  bool battleLost = false;

  Character character;

  private void Awake(){
    character = GetComponent<Character>();
  }

  private void Start(){
    SetFovRotation(character.Animator.DefaultDirection);
  }

  private void Update(){
    character.HandleUpdate();
  }

  public void Interact(Transform initiator){
    // trainer look to player and battle if player interact with them
    character.LookTowards(initiator.position);

    if(!battleLost){
      StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
        GameController.Instance.StartTrainerBattle(this);
      }));
    }else{
      StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
    }
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
      GameController.Instance.StartTrainerBattle(this);
    }));
  }

  public void BattleLost(){
    battleLost = true;
    fov.gameObject.SetActive(false);
  }

  // fov = field of view
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

  public string Name { get => name; }

  public Sprite Sprite { get => sprite; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable {
  public void onPlayerTriggered(PlayerController player){
    player.Character.Animator.IsMoving = false;
    
    GameController.Instance.OnEnterTrainersView(GetComponentInParent<TrainerController>());
  }
}

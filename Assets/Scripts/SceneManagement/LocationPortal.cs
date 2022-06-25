using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// to teleport player to a different position without switch scene
public class LocationPortal : MonoBehaviour, IPlayerTriggerable {
  [SerializeField] DestinationIdentifier destinationPortal;
  [SerializeField] Transform spawnPoint;

  PlayerController player;

  Fader fader;

  public void onPlayerTriggered(PlayerController player){
    player.Character.Animator.IsMoving = false;

    this.player = player;
    
    StartCoroutine(Teleport());
  }

  private void Start(){
    fader = FindObjectOfType<Fader>();
  }

  IEnumerator Teleport(){
    GameController.Instance.PauseGame(true);

    yield return fader.FadeIn(0.5f);

    var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
    player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

    yield return fader.FadeOut(0.5f);

    GameController.Instance.PauseGame(false);
  }

  public Transform SpawnPoint => spawnPoint;
}

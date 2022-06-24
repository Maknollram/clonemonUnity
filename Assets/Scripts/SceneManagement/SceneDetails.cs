using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour {
  public bool IsLoaded { get; private set; }

  [SerializeField] List<SceneDetails> connecctedScenes;

  private void OnTriggerEnter2D(Collider2D collision){
    if (collision.tag == "Player"){
      LoadScene();
      GameController.Instance.SetCurrentScene(this);

      // to load all connected scenes
      foreach (var scene in connecctedScenes){
        scene.LoadScene();
      }

      // to unload scenes not connected, comment to walk and see all the world on unity
      if (GameController.Instance.PrevScene != null){
        var previouslyLoadedScenes = GameController.Instance.PrevScene.connecctedScenes;
        foreach (var scene in previouslyLoadedScenes){
          if(!connecctedScenes.Contains(scene) && scene != this)
            scene.UnLoadScene();
        }
      }
    }
  }

  public void LoadScene(){
    if(!IsLoaded){
      SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
      IsLoaded = true;
    }
  }

  public void UnLoadScene(){
    if(IsLoaded){
      SceneManager.UnloadSceneAsync(gameObject.name);
      IsLoaded = false;
    }
  }
}

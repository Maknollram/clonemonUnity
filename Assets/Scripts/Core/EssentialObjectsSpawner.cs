﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour {
  [SerializeField] GameObject essentialObjectsPrefab;

  private void Awake(){
    var existingObjects = FindObjectsOfType<EssentialObjects>();

    if(existingObjects.Length == 0){
      // if have a grid spawn at it center
      var spawnPos = new Vector3(0, 0, 0);

      var grid = FindObjectOfType<Grid>();

      if(grid != null)
        spawnPos = grid.transform.position;
        
      Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
    }
  }
}

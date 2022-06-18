using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MoveSelectionUI : MonoBehaviour {
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

  [SerializeField] List<Text> moveTexts;
  [SerializeField] Color highlightedColor;

  int currentSelection = 0;

  public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove){
    for (int i = 0; i < currentMoves.Count; i++){ 
      moveTexts[i].text = currentMoves[i].Name;
    }

    // add new move to the list of moves the monster have
    moveTexts[currentMoves.Count].text = newMove.Name;
  }

  public void HandleMoveSelection(Action<int> onSelected){
    if (Input.GetKeyDown(joystick1 + DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
      ++currentSelection;
    else if(Input.GetKeyDown(joystick1 + UP) || Input.GetKeyDown(KeyCode.UpArrow))
      --currentSelection;

    currentSelection = Mathf.Clamp(currentSelection, 0, MonsterBase.MaxNumOfMoves);

    UpdateMoveSelection(currentSelection);

    if (Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2))
      onSelected?.Invoke(currentSelection);
  }

  public void UpdateMoveSelection(int selection){
    for (int i = 0; i < MonsterBase.MaxNumOfMoves + 1; i++){
      if(i == selection)
        moveTexts[i].color = highlightedColor;
      else
        moveTexts[i].color = Color.black;
    }
  }
}

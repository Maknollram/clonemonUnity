using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogManager : MonoBehaviour {
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

  [SerializeField] GameObject dialogBox;
  [SerializeField] Text dialogText;

  [SerializeField] int lettersPerSecond;

  public event Action OnShowDialog;
  public event Action OnCloseDialog;

  public static DialogManager Instance { get; private set; }
  
  Dialog dialog;
  int currentLine = 0;
  bool isTyping;

  public bool IsShowing { get; private set; }

  public void Awake()
  {
    Instance = this;
  }

  public IEnumerator ShowDialog(Dialog dialog){
    yield return new WaitForEndOfFrame();

    OnShowDialog?.Invoke();
    
    IsShowing = true;
    this.dialog = dialog;
    dialogBox.SetActive(true);
    StartCoroutine(TypeDialog(dialog.Lines[0]));
  }

  public void HandleUpdate(){
    if ((Input.GetKeyDown(joystick1 + CROSS) || Input.GetKeyDown(KeyCode.Keypad2)) && !isTyping){
      ++currentLine;
      if(currentLine < dialog.Lines.Count){
        StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
      } else{
        currentLine = 0;

        IsShowing = false;
        dialogBox.SetActive(false);
        OnCloseDialog?.Invoke();
      }
    }
  }

  public IEnumerator TypeDialog(string line)
  {
    isTyping = true;
    dialogText.text = "";
    foreach (var letter in line.ToCharArray())
    {
      dialogText.text += letter;
      yield return new WaitForSeconds(1f/lettersPerSecond);
    }
    isTyping = false;
  }
}

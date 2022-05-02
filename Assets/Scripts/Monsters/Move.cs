using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{

	public MoveBase Base {get; set;}

  public int SP { get; set;}

  public Move(MoveBase mBase)
  {
    Base = mBase;
    SP = mBase.SP;
  }
}

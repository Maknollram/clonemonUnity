using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition {
  public ConditionID Id  { get; set; }
  public string Name { get; set; }
  public string Description { get; set; }
  public string StartMessage { get; set; }

  public Action<Monster> OnStart { get; set; }

  public Func<Monster, bool> OnBeforeMove { get; set; } // happens before the monster use a move and block its move

  public Action<Monster> OnAfterTurn { get; set; } // happens after the monster use a move
}

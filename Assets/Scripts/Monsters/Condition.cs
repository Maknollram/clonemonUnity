﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition {
  public string Name { get; set; }
  public string Description { get; set; }
  public string StartMessage { get; set; }

  public Action<Monster> OnStart { get; set; }

  public Func<Monster, bool> OnBeforeMove { get; set; }

  public Action<Monster> OnAfterTurn { get; set; }
}

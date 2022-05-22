﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]
public class MoveBase : ScriptableObject {

  [SerializeField] string name;

  [TextArea]
  [SerializeField] string description;

  [SerializeField] MonsterType type;
  [SerializeField] int power;
  [SerializeField] int accuracy;
  [SerializeField] int sp;

  // [SerializeField] bool isSpecial;

  [SerializeField] MoveCategory category;

  [SerializeField] MoveEffects effects;

  [SerializeField] MoveTarget target;

  public string Name { get { return name; } }

  public string Description { get { return description; } }

  public MonsterType Type { get { return type; } }

  public int Power { get { return power; } }

  public int Accuracy { get { return accuracy; } }

  public int SP { get { return sp; } }

  public MoveCategory Category { get { return category; } }

  public MoveEffects Effects { get { return effects; } }

  public MoveTarget Target { get { return target; } }

  // public bool IsSpecial { get { return isSpecial; } } 
  }

  [System.Serializable]
  public class MoveEffects {
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> Boosts{ get { return boosts; } }
  }

  [System.Serializable]
  public class StatBoost{
    public Stat stat;
    public int boost;
  }

  public enum MoveCategory { Physical, Special, Status }

  public enum MoveTarget { Foe, Self }

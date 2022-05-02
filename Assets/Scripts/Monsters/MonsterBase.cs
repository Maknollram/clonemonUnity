using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName ="Monster/Create new monster")]
public class MonsterBase : ScriptableObject {

	[SerializeField] string name;

  [TextArea]
  [SerializeField] string description;

  [SerializeField] Sprite frontSprite;
  [SerializeField] Sprite backSprite;

  [SerializeField] MonsterType type1;
  [SerializeField] MonsterType type2;

  // Base Stats
  [SerializeField] int maxHP;
  [SerializeField] int attack;
  [SerializeField] int defense;
  [SerializeField] int spAttack;
  [SerializeField] int spDefense;
  [SerializeField] int speed;

  [SerializeField] List<LearnableMove> learnableMoves;

  public string Name {
    get { return name; }
  }

  public string Description {
    get { return description; }
  }

  public Sprite FrontSprite {
    get { return frontSprite; }
  }

  public Sprite BackSprite {
    get { return backSprite; }
  }

  public MonsterType Type1 {
    get { return type1; }
  }

  public MonsterType Type2 {
    get { return type2; }
  }

  public int MaxHp {
    get { return maxHP; }
  }

  public int Attack {
    get { return attack; }
  }

  public int Defense {
    get { return defense; }
  }

  public int SpAttack {
    get { return spAttack; }
  }

  public int SpDefense {
    get { return spDefense; }
  }

  public int Speed {
    get { return speed; }
  }

  public List<LearnableMove> LearnableMoves {
    get { return learnableMoves; }
  }
}

[System.Serializable]
public class LearnableMove
{
  [SerializeField] MoveBase moveBase;
  [SerializeField] int level;

  public MoveBase Base {
    get { return moveBase; }
  }

  public int Level {
    get { return level; }
  }
}

public enum MonsterType
{
  // None,
  // Bug,
  // Dark, 
  // Dragon,
  // Eletric,
  // Fairy,
  // Fighting,
  // Fire,
  // Flying,
  // Ghost,
  // Grass,
  // Ground,
  // Ice,
  // Normal,
  // Poison,
  // Psychic,
  // Rock,
  // Steel,
  // Water
  None,
  Água,
  Dragão,
  Elétrico,
  Fada,
  Fantasma,
  Fogo,
  Gelo,
  Inseto,
  Lutador,
  Metálico,
  Normal,
  Pedra,
  Planta,
  Psíquico,
  Sombrio, 
  Terra,
  Venenoso,
  Voador
}

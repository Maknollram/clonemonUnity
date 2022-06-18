using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

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

  [SerializeField] int expYield;
  [SerializeField] GrowthRate growthRate;

  [SerializeField] int catchRate = 255;

  [SerializeField] List<LearnableMove> learnableMoves;

  public static int MaxNumOfMoves { get; set; } = 4;

  public int GetExpForLevel(int level){
    if(growthRate == GrowthRate.Fast)
      return 4 * (level * level * level) / 5;
    else if(growthRate == GrowthRate.MediumFast)
      return level * level * level;
    else if (growthRate == GrowthRate.MediumSlow)
      return 6 * (level * level * level) / 5 - 15 * (level * level) + 100 * level - 140;
    else if (growthRate == GrowthRate.Slow)
      return 5 * (level * level * level) / 4;
    else if (growthRate == GrowthRate.Fluctuating)
      return GetFluctuating(level);
    else if (growthRate == GrowthRate.None)
      return  level;
      
    return -1;
  }

   public int GetFluctuating(int level){
    if (level <= 15){
      return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor((level + 1) / 3) + 24) / 50));
    }else if (level >= 15 && level <= 36){
      return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level + 14) / 50));
    }else{
      return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(level / 2) + 32) / 50));
    }
  }

  public string Name { get { return name; } }

  public string Description { get { return description; } }

  public Sprite FrontSprite { get { return frontSprite; } }

  public Sprite BackSprite { get { return backSprite; } }

  public MonsterType Type1 { get { return type1; } }

  public MonsterType Type2 { get { return type2; } }

  public int MaxHp { get { return maxHP; } }

  public int Attack { get { return attack; } }

  public int Defense { get { return defense; } }

  public int SpAttack { get { return spAttack; } }

  public int SpDefense { get { return spDefense; } }

  public int Speed { get { return speed; } }

  public int CatchRate => catchRate;

  public int ExpYield => expYield;

  public GrowthRate GrowthRate => growthRate;

  public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }
}

[System.Serializable]
public class LearnableMove{
  [SerializeField] MoveBase moveBase;
  [SerializeField] int level;

  public MoveBase Base { get { return moveBase; } }

  public int Level { get { return level; } }
}

public enum MonsterType{
  // None,
  // Normal,
  // Fire,
  // Water,
  // Eletric,
  // Grass,
  // Ice,
  // Fighting,
  // Poison,
  // Ground,
  // Flying,
  // Psychic,
  // Bug,
  // Rock,
  // Ghost,
  // Dragon,
  // Dark, 
  // Steel,
  // Fairy

  None,
  Choque,/*elehtrico*//*virtual*/
  Deus,
  Fluido,/*ahgua*//*gelo*/
  Fogo,
  Inseto,
  Lutador,
  Luz,/*fada*/
  Metal,/*mahquina*//*mecahnico*/
  Negro,/*fantasma*/
  Neutro,
  Planta,
  Terra,/*pedra*/
  Voador
}

public enum GrowthRate{
  [Description("Normal")]
  None,
  [Description("Muito Rápido")]
  Fast,
  [Description("Rápido")]
  MediumFast,
  [Description("Lento")]
  MediumSlow,
  [Description("Muito Lento")]
  Slow,
  [Description("Flutuante")]
  Fluctuating
}

// Description precisa usar System.ComponentModel e eh usado no .DisplayName(), exemplo na class Monster
public enum Stat{
  [Description("Ataque")]
  Attack,
  [Description("Defesa")]
  Defense,
  [Description("Ataque Mágico")]
  SpAttack,
  [Description("Defesa Mágica")]
  SpDefense,
  [Description("Agilidade")]
  Speed,
  // only to be increased or decrease on battle
  [Description("Precisão")]
  Accuracy,
  [Description("Evasão")]
  Evasion
}

public class TypeChart{
  static float [][] chart = { 
    //Has to be same order as PokemonType class
    //                       Nor   Fir   Wat   Ele   Gra   Ice   Fig   Poi   Gro   Fly   Psy   Bug   Roc   Gho   Dra   Dar  Ste    Fai
    // /*Normal*/  new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 0,    1f,   1f,   0.5f, 1f},
    // /*Fire*/    new float[] {1f,   0.5f, 0.5f, 1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 1f,   2f,   1f},
    // /*Water*/   new float[] {1f,   2f,   0.5f, 1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   1f,   1f},
    // /*Electric*/new float[] {1f,   1f,   2f,   0.5f, 0.5f, 1f,   1f,   1f,   0f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f},
    // /*Grass*/   new float[] {1f,   0.5f, 2f,   1f,   0.5f, 1f,   1f,   0.5f, 2f,   0.5f, 1f,   0.5f, 2f,   1f,   0.5f, 1f,   0.5f, 1f},
    // /*Ice*/     new float[] {1f,   0.5f, 0.5f, 1f,   2f,   0.5f, 1f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f},
    // /*Fighting*/new float[] {2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f, 0.5f, 0.5f, 2f,   0f,   1f,   2f,   2f,   0.5f},
    // /*Poison*/  new float[] {1f,   1f,   1f,   1f,   2f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   0f,   2f},
    // /*Ground*/  new float[] {1f,   2f,   1f,   2f,   0.5f, 1f,   1f,   2f,   1f,   0f,   1f,   0.5f, 2f,   1f,   1f,   1f,   2f,   1f},
    // /*Flying*/  new float[] {1f,   1f,   1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 1f},
    // /*Psychic*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f,   0.5f, 1f,   1f,   1f,   1f,   0f,   0.5f, 1f},
    // /*Bug*/     new float[] {1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 0.5f, 1f,   0.5f, 2f,   1f,   1f,   0.5f, 1f,   2f,   0.5f, 0.5f},
    // /*Rock*/    new float[] {1f,   2f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f},
    // /*Ghost*/   new float[] {0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 1f,   1f},
    // /*Dragon*/  new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 0f},
    // /*Dark*/    new float[] {1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f},
    // /*Steel*/   new float[] {1f,   0.5f, 0.5f, 0.5f, 1f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 2f},
    // /*Fairy*/   new float[] {1f,   0.5f, 1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   0.5f, 1f},
    
    // segue o objeto MonsterType
    // 0.5f atq não efetivo / 1f atq normal / 1.25f atq somente para deuses / 1.5f atq super efetivo
    //                    cho     deu     flu     fog     ins     lut     luz     met     neg     neu     pla     ter     voa
    /*cho*/ new float[] { 0.5f,   0.5f,   1.5f,   1f,     1f,     1.5f,   0f,     1.5f,   1f,     1f,     1f,     0f,     1.5f},
    /*deu*/ new float[] { 1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f,  1.25f},
    /*flu*/ new float[] { 0f,     0.5f,   0.5f,   1.5f,   1.5f,   1f,     1f,     1f,     1f,     1f,     0f,     0f,     1.5f},
    /*fog*/ new float[] { 1f,     0.5f,   0f,     0.5f,   1.5f,   1.5f,   0f,     1.5f,   1f,     1f,     1.5f,   0f,     0.5f},
    /*ins*/ new float[] { 1f,     0.5f,   0f,     0f,     0.5f,   1.5f,   0f,     0f,     1f,     1f,     1.5f,   1f,     1f},
    /*lut*/ new float[] { 0f,     0.5f,   1f,     0f,     1.5f,   0.5f,   1f,     1f,     0f,     1f,     1.5f,   1f,     0f},
    /*luz*/ new float[] { 0f,     0.5f,   1f,     0f,     1f,     1f,     0.5f,   1.5f,   1.5f,   1f,     0.5f,   1f,     1f},
    /*met*/ new float[] { 0f,     0.5f,   1f,     0f,     1.5f,   1f,     0f,     0.5f,   0f,     1f,     1.5f,   1f,     1.5f},
    /*neg*/ new float[] { 0.5f,   0.5f,   1f,     0f,     1f,     1.5f,   1.5f,   0f,     0.5f,   1f,     1.5f,   1f,     1.5f},
    /*neu*/ new float[] { 1f,     0.5f,   1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f,     1f},
    /*pla*/ new float[] { 1f,     0.5f,   1.5f,   0f,     0.5f,   0f,     1.5f,   0f,     0.5f,   1f,     0.5f,   1.5f,   1f},
    /*ter*/ new float[] { 0f,     0.5f,   1.5f,   1.5f,   1f,     0f,     1f,     1f,     1f,     1f,     1.5f,   0.5f,   1.5f},
    /*voa*/ new float[] { 0.5f,   0.5f,   0f,     1f,     1.5f,   1.5f,   1f,     0f,     0f,     1f,     1.5f,   1f,     0.5f,}
  };

  public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType){ 
    if (attackType == MonsterType.None || defenseType == MonsterType.None)
      return 1;

    int row = (int)attackType - 1;
    int col = (int)defenseType - 1;

    return chart[row][col];
  }
}
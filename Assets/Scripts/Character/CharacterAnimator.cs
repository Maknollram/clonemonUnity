using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour {
  [SerializeField] List<Sprite> walkUpSprites;
  [SerializeField] List<Sprite> walkDownSprites;
  [SerializeField] List<Sprite> walkRightSprites;
  [SerializeField] List<Sprite> walkLeftSprites;
  [SerializeField] List<Sprite> walkUpRightSprites;
  [SerializeField] List<Sprite> walkDownRightSprites;
  [SerializeField] List<Sprite> walkUpLeftSprites;
  [SerializeField] List<Sprite> walkDownLeftSprites;

  [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;
  
  //parameters
  public float MoveX { get; set; }
  public float MoveY { get; set; }
  public bool IsMoving { get; set; }

  // states
  SpriteAnimator walkUpAnim;
  SpriteAnimator walkDownAnim;
  SpriteAnimator walkRightAnim;
  SpriteAnimator walkLeftAnim;
  SpriteAnimator walkUpRightAnim;
  SpriteAnimator walkDownRightAnim;
  SpriteAnimator walkUpLeftAnim;
  SpriteAnimator walkDownLeftAnim;

  SpriteAnimator currentAnim;

  bool wasPreviouslyMoving;

  // references
  SpriteRenderer spriteRenderer;

  private void Start(){
    spriteRenderer = GetComponent<SpriteRenderer>();
    walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
    walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
    walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
    walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
    walkUpRightAnim = new SpriteAnimator(walkUpRightSprites, spriteRenderer);
    walkDownRightAnim = new SpriteAnimator(walkDownRightSprites, spriteRenderer);
    walkUpLeftAnim = new SpriteAnimator(walkUpLeftSprites, spriteRenderer);
    walkDownLeftAnim = new SpriteAnimator(walkDownLeftSprites, spriteRenderer);
    SetFacingDirection(defaultDirection);

    currentAnim = walkDownAnim;
  }

  private void Update(){
    var prevAnim = currentAnim;

    if(MoveX == 1 && MoveY == 1)
      currentAnim = walkUpRightAnim;
    else if(MoveX == -1 && MoveY == 1)
      currentAnim = walkUpLeftAnim;
    else if(MoveX == 1 && MoveY == -1)
      currentAnim = walkDownRightAnim;
    else if(MoveX == -1 && MoveY == -1)
      currentAnim = walkDownLeftAnim;
    else if(MoveX == 1)
      currentAnim = walkRightAnim;
    else if(MoveX == -1)
      currentAnim = walkLeftAnim;
    else if(MoveY == 1)
      currentAnim = walkUpAnim;
    else if(MoveY == -1)
      currentAnim = walkDownAnim;

    if(currentAnim != prevAnim || IsMoving != wasPreviouslyMoving)
      currentAnim.Start();

    if(IsMoving)
      currentAnim.HandleUpdate();
    else
      spriteRenderer.sprite = currentAnim.Frames[0];

    wasPreviouslyMoving = IsMoving;
  }

  public void SetFacingDirection(FacingDirection dir){
    if(dir == FacingDirection.UpRight){
      MoveX = 1;
      MoveY = 1;
    }
    else if(dir == FacingDirection.UpLeft){
      MoveX = -1;
      MoveY = 1;
    }
    else if(dir == FacingDirection.DownRight){
      MoveX = 1;
      MoveY = -1;
    }
    else if(dir == FacingDirection.DownLeft){
      MoveX = -1;
      MoveY = -1;
    }
    else if(dir == FacingDirection.Right)
      MoveX = 1;
    else if(dir == FacingDirection.Left)
      MoveX = -1;
    else if(dir == FacingDirection.Up)
      MoveY = 1;
    else if(dir == FacingDirection.Down)
      MoveY = -1;
  }

  public FacingDirection DefaultDirection{ get => defaultDirection; }
}

public enum FacingDirection {
  Up,
  Down,
  Left,
  Right,
  UpRight,
  DownRight,
  UpLeft,
  DownLeft
}

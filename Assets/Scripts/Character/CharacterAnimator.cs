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
}

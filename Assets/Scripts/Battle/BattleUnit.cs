using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour {
  [SerializeField] bool isPlayerUnit;

  public Monster Monster { get; set; }

  Image image;
  Vector3 originalPos;
  Color originalColor;

  private void Awake()
  {
    image = GetComponent<Image>();
    originalPos = image.transform.localPosition;
    originalColor = image.color;
  }

  public void Setup(Monster monster)
  {
    Monster = monster;
    if (isPlayerUnit)
      image.sprite = Monster.Base.BackSprite;
    else
      image.sprite = Monster.Base.FrontSprite;

    image.color = originalColor;
    PlayEnterAnimation();
  }

  public void PlayEnterAnimation()
  {
    if (isPlayerUnit)
    {
      image.transform.localPosition = new Vector3(originalPos.x, -403f);
      image.transform.DOLocalMoveY(originalPos.y, 1f);
    }
    else
    {
      image.transform.localPosition = new Vector3(506f, originalPos.y);
      image.transform.DOLocalMoveX(originalPos.x, 1f);
    }
  }

  public void PlayAttackAnimation()
  {
    var sequence = DOTween.Sequence();
    if(isPlayerUnit)
      sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
    else
      sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

    sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
  }

  public void PlayHitAnimation()
  {
    var sequence = DOTween.Sequence();
    sequence.Append(image.DOColor(Color.red, 0.1f));
    sequence.Append(image.DOColor(originalColor, 0.1f));
    
    if(isPlayerUnit)
      sequence.Join(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
    else
      sequence.Join(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));

    sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
  }

  public void PlayFaintAnimation()
  {
    if(isPlayerUnit)
      image.transform.DOLocalMoveY(originalPos.y - 150f, 0.25f);
    else
      image.DOFade(0f, 0.5f);
  }

  public void PlayVictoryAnimation()
  {
    var sequence = DOTween.Sequence();
    for (int i = 0; i < 2; i++)
    {
      if(isPlayerUnit)
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 25f, 0.15f));
      else
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y + 25f, 0.15f));

      sequence.Append(image.transform.DOLocalMoveY(originalPos.y, 0.15f));
    }
  }
}

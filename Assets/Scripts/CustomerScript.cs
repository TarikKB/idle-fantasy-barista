using UnityEngine;
using DG.Tweening;

public class CustomerScript : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float jumpHeight = 0.10f;
    [SerializeField] float jumpDuration = 0.125f;
    Tween jumpTween;

    public void MoveUpLine(Transform nextPoint)
    {
        transform.DOKill();

        float dist = Vector3.Distance(transform.position, nextPoint.position);
        float moveDuration = dist / speed;
        float baseY = transform.position.y;

        transform.DOMove(nextPoint.position, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (jumpTween != null && jumpTween.IsActive())
                    jumpTween.Kill();

                transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
            });

        jumpTween = transform.DOMoveY(baseY + jumpHeight, jumpDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void ExitLine(Transform exitPoint)
    {
        transform.DOKill();

        float dist = Vector3.Distance(transform.position, exitPoint.position);
        float moveDuration = dist / speed;
        float baseY = transform.position.y;

        transform.DOMove(exitPoint.position, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (jumpTween != null && jumpTween.IsActive())
                    jumpTween.Kill();

                transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
                Destroy(gameObject);
            });

        jumpTween = transform.DOMoveY(baseY + jumpHeight, jumpDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
        

    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardBehaviour : MonoBehaviour
{
    private int spriteID;
    private int id;
    private bool flipped;
    private bool turning;
    private bool isInactive;
    [SerializeField] private Image img;

    private IEnumerator Flip180(Transform thisTransform, float time, bool changeSprite) {
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation*Quaternion.Euler(0, 180, 0);
        float rate = 2.0f/time;
        float t = 0.0f;

        while (t<1.0f) {
            t+=Time.deltaTime*rate;
            thisTransform.rotation=Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        if (changeSprite) {
            flipped=!flipped;
            ChangeSprite();
            StartCoroutine(Flip180(transform, time, false));
        }
        else {
            turning=false;
        }
    }

    public void Flip() {
        turning=true;
        AudioPlayer.Instance.PlayAudio(0);
        StartCoroutine(Flip180(transform, 0.25f, true));
    }

    private void ChangeSprite() {
        if (spriteID==-1||img==null)
            return;
        img.sprite=flipped
            ? GameController.Instance.GetSprite(spriteID)
            : GameController.Instance.CardBack();
    }

    public void Inactive() {
        isInactive=true;
        StartCoroutine(Fade());
    }

    private IEnumerator Fade() {
        float rate = 1.0f/2.5f;
        float t = 0.0f;

        while (t<1.0f) {
            t+=Time.deltaTime*rate;
            img.color=Color.Lerp(img.color, Color.clear, t);
            yield return null;
        }
    }

    public void Active() {
        if (img) {
            isInactive=false;
            img.color=Color.white;
        }
    }

    public int SpriteID {
        set {
            spriteID=value;
            flipped=true;
            ChangeSprite();
        }
        get => spriteID;
    }

    public int ID {
        set => id=value;
        get => id;
    }

    public bool Flipped {
        set => flipped=value;
        get => flipped;
    }

    public bool IsInactive {
        set => isInactive=value;
        get => isInactive;
    }

    public void ResetRotation() {
        transform.rotation=Quaternion.Euler(0, 360, 0);
        flipped=true;
    }

    public void CardBtn() {
        if (flipped||turning||!GameController.Instance.CanClick())
            return;
        Flip();
        StartCoroutine(SelectionEvent());
    }

    private IEnumerator SelectionEvent() {
        yield return new WaitForSeconds(0.5f);
        GameController.Instance.CardClicked(spriteID, id);
    }
}

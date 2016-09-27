using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Pot : MonoBehaviour {
    public int score;
    private Animation lightAnim;
    private Animator splashAnimator;
    private int idleStateHash = Animator.StringToHash("Base.Idle");
    private int playHash = Animator.StringToHash("Play");

    void Start()
    {
        lightAnim = transform.FindChild("Light").GetComponent<Animation>();
        splashAnimator = transform.FindChild("Splash").GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        OnTriggerEnter2D(coll.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.GetComponent<GameItem>().itemType == GameItem.ItemType.Ball)
        {
            Ball ball = otherGO.GetComponent<Ball>();
            if (ball.state == Ball.BallState.Dropped)
            {
                PlaySplashAnim(ball);
            }
        }
                
    }

    void PlaySplashAnim(Ball ball)
    {
        ball.SplashDestroy();

        if (!lightAnim.isPlaying && splashAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == idleStateHash)
        {
            lightAnim.Play();
            splashAnimator.SetTrigger(playHash);
        }

        int potScore = ScoreManager.Instance.UpdatePotScore(score);
        ScoreManager.Instance.PopupPotScore( potScore, transform.position + Vector3.up );
        SoundManager.Instance.Play(SoundSeqType.BallFallInPot);
        /*
        StartCoroutine( SoundsCounter() );
        if( mainscript.Instance.potSounds < 4 )
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.pops );

        GameObject splash = (GameObject)Instantiate(splashPrefab, transform.position + Vector3.up * 0.9f + Vector3.left * 0.35f, Quaternion.identity);
        Destroy(splash, 2f);

        */
    }

    IEnumerator SoundsCounter()
    {
        mainscript.Instance.potSounds++;
        yield return new WaitForSeconds( 0.2f );
        mainscript.Instance.potSounds--;
    }
}

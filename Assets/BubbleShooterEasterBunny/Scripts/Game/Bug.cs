﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bug : MonoBehaviour {
    Transform spiders;
    public Sprite[] textures;
    public int color;
    int score = 25;
	// Use this for initialization
    private GameObject bugSprite;

    void Awake () {
        bugSprite = transform.Find("BugSprite").gameObject;
    }

	void Start () {
        spiders = GameObject.Find( "Spiders" ).transform;
        ChangeColor( 0 );
        if(ScoreManager.Instance.ComboCount % 3 == 0 && ScoreManager.Instance.ComboCount > 0 ) ChangeColor( 1 );
        if(ScoreManager.Instance.ComboCount % 5 == 0 && ScoreManager.Instance.ComboCount > 0 ) ChangeColor( 2 );
        SelectPlace();
	}
	
	// Update is called once per frame
    void SelectPlace()
    {
        List<Transform> listFreePlaces = new List<Transform>();
        foreach( Transform item in spiders )
        {
            if( item.childCount == 0 ) listFreePlaces.Add( item );
        }
        StartCoroutine( MoveToPlace( listFreePlaces[Random.Range( 0, listFreePlaces.Count )] ) );
    }

    IEnumerator MoveToPlace( Transform place )
    {
        transform.parent = place;
        AnimationCurve curveX = new AnimationCurve( new Keyframe( 0, transform.localPosition.x ), new Keyframe( 1, 0 ) );
        AnimationCurve curveY = new AnimationCurve( new Keyframe( 0, transform.localPosition.y ), new Keyframe( 1, 0 ) );
        curveY.AddKey( 0.5f, transform.localPosition.y + 3 );
        float startTime = Time.time;
        Vector3 startPos = transform.localPosition;
        float speed = Random.Range(0.4f,0.6f);
        float distCovered = 0;
        while( distCovered < 1 )
        {
            if(distCovered >0.8f && distCovered < 0.81f)
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.swish[0] );
            distCovered = ( Time.time - startTime ) * speed;
            transform.localPosition = new Vector3( curveX.Evaluate( distCovered ), curveY.Evaluate( distCovered ), 0 );

            yield return new WaitForEndOfFrame();
        }
        GetComponent<BoxCollider2D>().isTrigger = false;
    }

    public void MoveOut()
    {
        transform.parent = null;
        StartCoroutine( MoveOutCor() );
    }

    IEnumerator MoveOutCor()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.bugDissapier );

        AnimationCurve curveY = new AnimationCurve( new Keyframe( 0, transform.localPosition.y ), new Keyframe( 1, 10 ) );
        curveY.AddKey( 0.5f, transform.localPosition.y + 3 );
        float startTime = Time.time;
        Vector3 startPos = transform.localPosition;
        float speed = 0.6f;
        float distCovered = 0;
        while( distCovered < 1 )
        {
            distCovered = ( Time.time - startTime ) * speed;
            transform.localPosition = new Vector3( transform.localPosition.x, curveY.Evaluate( distCovered ), 0 );

            yield return new WaitForEndOfFrame();
        }
        transform.parent = null;
        Destroy( gameObject );
    }

    public void ChangeColor( int i )
    {
        color = i;
        bugSprite.GetComponent<SpriteRenderer>().sprite = textures[i];
    }

    void OnCollisionEnter2D( Collision2D col )
    {
        if( col.collider.name.Contains( "ball" ) )
        {
            StartCoroutine( SoundsCounter() );
            if( CoreManager.Instance.bugSounds < 5)
            {
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.bug );
            }
            col.collider.GetComponent<Ball>().HitBug++;
            int bugScore = ScoreManager.Instance.UpdateBugScore(col.collider.GetComponent<Ball>().HitBug * score * (color + 1));
            ScoreManager.Instance.PopupComboScore( bugScore, transform.position);
            StartCoroutine( StartAnim( col.collider.transform.position ) );
            // 这段代码是干啥用的？导致了exception，暂时注释掉
            //if( color == 2 )
            //{
            //    SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.combo[5]);
            //    ChangeColor( 1 );
            //    GameObject ball = GameItemFactory.Instance.CreateFixedBall(transform.position + Vector3.up * 1, new LevelGameItem(LevelItemType.Random));
            //    ball.GetComponent<Ball>().StartFall();
            //}

        }
    }

    IEnumerator SoundsCounter()
    {
        CoreManager.Instance.bugSounds++;
        yield return new WaitForSeconds( 0.3f );
        CoreManager.Instance.bugSounds--;
    }

    IEnumerator StartAnim(Vector3 dir)
    {
        AnimationCurve curveScale = new AnimationCurve( new Keyframe( 0, 1 ), new Keyframe( 1, 0.8f ) );
        AnimationCurve curveScaleReverse = new AnimationCurve( new Keyframe( 0, 0.8f ), new Keyframe( 1, 1 ) );
        AnimationCurve curveScaleX = new AnimationCurve( new Keyframe( 0, 1 ), new Keyframe( 1, 1.2f ) );
        AnimationCurve curveScaleReverseX = new AnimationCurve( new Keyframe( 0, 1.2f ), new Keyframe( 1, 1 ) );

        dir = transform.position - dir;
        dir = dir.normalized;
        dir = transform.localPosition + ( dir * 0.5f );

        float startTime = Time.time;
        Vector3 startPos = transform.localPosition;
        float speed = 25;
        float distCovered = 0;
        while( distCovered < 0.5f && !float.IsNaN( dir.x ) )
        {
            distCovered = ( Time.time - startTime ) * speed;
            if( this == null ) yield break;
            transform.localPosition = Vector3.Lerp( startPos, dir, distCovered );
            transform.localScale = new Vector3( curveScaleX.Evaluate( distCovered ), curveScale.Evaluate( distCovered ), 1 );
            yield return new WaitForEndOfFrame();
        }
        Vector3 lastPos = transform.localPosition;
        startTime = Time.time;
        distCovered = 0;
        while( distCovered < 0.5f && !float.IsNaN( dir.x ) )
        {
            distCovered = ( Time.time - startTime ) * speed;
            if( this == null ) yield break;
            transform.localPosition = Vector3.Lerp( lastPos, Vector3.zero, distCovered );
            transform.localScale = new Vector3( curveScaleReverseX.Evaluate( distCovered ), curveScaleReverse.Evaluate( distCovered ), 1 );
            yield return new WaitForEndOfFrame();
        }
    }
}

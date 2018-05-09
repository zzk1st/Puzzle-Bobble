using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollViewController : MonoBehaviour
{
	public ScrollRect sr;
	public Button btnL, btnR;
	public Toggle[] toggles;
	private int indicatorIndex = 0;
	private int indicatorLength = 2;
	private float gesturePosX;

	void Start ()
	{
		sr.onValueChanged.AddListener (new UnityEngine.Events.UnityAction<Vector2> (
			(Vector2 v) => {
//				print (v.ToString ());
			}
		));


		btnL.onClick.AddListener (new UnityEngine.Events.UnityAction (() => {
			ScrollLeft ();
		}));
		btnR.onClick.AddListener (() => {
			ScrollRight ();
		});
	}

	void Update ()
	{
		
	}

	public void StartGesture ()
	{
		print ("StartGesture");
		gesturePosX = sr.content.localPosition.x;
	}

	public void EndGesture ()
	{
		print ("EndGesture:" + (sr.content.localPosition.x - gesturePosX).ToString ());
		if (sr.content.localPosition.x - gesturePosX < -500f) {
			//左滑动
			ScrollLeft ();
		} else if (sr.content.localPosition.x - gesturePosX > 500f) {
			//右滑动
			ScrollRight ();
		} else {
			//原地
			ScrollStay ();
		}
		gesturePosX = sr.content.localPosition.x;
	}

	//滑动翻页Left
	private void ScrollLeft ()
	{
		if (indicatorIndex < indicatorLength) {
//			sr.content.localPosition = new Vector2 (Vector2.zero.x - sr.content.rect.width / 3 * indicatorIndex, 0f);
//			DOTween.To (() => sr.normalizedPosition, x => sr.normalizedPosition = x, new Vector2 (Vector2.zero.x - sr.content.rect.width / 3 * indicatorIndex, 0f), 3);
			DOTween.To (() => sr.content.anchoredPosition, x => sr.content.anchoredPosition = x, new Vector2 (Vector2.zero.x - sr.content.rect.width / (indicatorLength + 1) * (indicatorIndex + 1), 0f), 0.6f).OnComplete (() => {
				indicatorIndex++;
				toggles [indicatorIndex].isOn = true;
			});
		}
	}
	//滑动翻页Right
	private void ScrollRight ()
	{
		if (indicatorIndex > 0) {
			print ("right 1");
//			sr.content.localPosition = new Vector2 (Vector2.zero.x - sr.content.rect.width / 3 * indicatorIndex, 0f);
			DOTween.To (() => sr.content.anchoredPosition, x => sr.content.anchoredPosition = x, new Vector2 (Vector2.zero.x - sr.content.rect.width / (indicatorLength + 1) * (indicatorIndex - 1), 0f), 0.6f).OnComplete (() => {
				indicatorIndex--;
				toggles [indicatorIndex].isOn = true;
			});
		}
	}

	//原地滑动
	private void ScrollStay ()
	{
//		sr.content.localPosition = new Vector2 (Vector2.zero.x - sr.content.rect.width / 3 * indicatorIndex, 0f);
		DOTween.To (() => sr.content.anchoredPosition, x => sr.content.anchoredPosition = x, new Vector2 (Vector2.zero.x - sr.content.rect.width / (indicatorLength + 1) * indicatorIndex, 0f), 0.2f).OnComplete (() => {
		});
	}
}

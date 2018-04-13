using UnityEngine;
using System.Collections;

public class MagicBallBoost : MonoBehaviour {
    BallColor currentColor;
    Vector3 mousePos;
    private bool setColor = false;
    private bool findColor = false;

    // Use this for initialization
    void Start () {
        // 游戏进入暂停状态
        UIManager.Instance.Pause();
    }
	

	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            // 如果仅剩余一个颜色，抱歉没法再消除了
            if (mainscript.Instance.curStageColors.Count == 1 || currentColor == 0)
                return;
            // 消除该颜色
            BallColor newColor = BallColor.blue;
            BallColor catapultColor = mainscript.Instance.ballShooter.CatapultBall.GetComponent<Ball>().color;
            BallColor cartridgeColor = mainscript.Instance.ballShooter.CartridgeBall.GetComponent<Ball>().color;
            if (catapultColor == cartridgeColor && catapultColor == currentColor )
            {
                // 如果那两个都一样 而且都是当前选中的颜色
                // 那么在其余颜色里随机选一个 同时把那两个
                // 的颜色也改变为那个随机颜色
                foreach (BallColor color in mainscript.Instance.curStageColors)
                {
                    if (color != catapultColor)
                    {
                        newColor = color;
                        break;
                    }
                }
                //更新颜色
                mainscript.Instance.UpdateColorsInGame(newColor, currentColor);
                mainscript.Instance.ballShooter.CatapultBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
                mainscript.Instance.ballShooter.CartridgeBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
            }
            else if (catapultColor == currentColor || catapultColor == cartridgeColor || cartridgeColor == currentColor)
            {
                if (catapultColor == currentColor)
                {
                    mainscript.Instance.curStageColors.Remove(currentColor);
                    newColor = mainscript.Instance.curStageColors[Random.Range(0, mainscript.Instance.curStageColors.Count)];
                    mainscript.Instance.UpdateColorsInGame(newColor, currentColor);
                    mainscript.Instance.ballShooter.CatapultBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
                }
                else
                {
                    newColor = catapultColor;
                    //更新颜色
                    mainscript.Instance.UpdateColorsInGame(newColor, currentColor);
                    mainscript.Instance.ballShooter.CatapultBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
                    mainscript.Instance.ballShooter.CartridgeBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
                }
            }
            else
            {
                // 首选Catapult和Cartridge上的颜色 (随机一半一半)
                int isCatapult = Random.Range(0, 1);
                newColor = isCatapult>0 ? catapultColor : cartridgeColor;
                //更新颜色
                mainscript.Instance.UpdateColorsInGame(newColor, currentColor);
                if (isCatapult>0)
                    mainscript.Instance.ballShooter.CatapultBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
                else
                    mainscript.Instance.ballShooter.CartridgeBall.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
            }
            setColor = true;
        }

        if (Input.GetMouseButtonUp(0) && setColor)
        {
            //游戏状态改回Play
            UIManager.Instance.Resume();
            //销毁自己
            Destroy(gameObject);
        }


        // 判断当前鼠标位置 改变currentColor

        // 获取当前鼠标位置
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        
        // 遍历所有fixedball 找到和当前鼠标overlay的(距离小于ball的半径)
        // tie breaker就是谁先找到算谁的 完了马上退出遍历
        findColor = false;
        foreach (Transform gameItemTransform in mainscript.Instance.gameItemsNode.transform)
        {
            Ball ball = gameItemTransform.gameObject.GetComponent<Ball>();
            
            if (ball != null)
            {
                Vector3 ballPos = ball.transform.position;
                ballPos.z = 0f;
                float dist = Vector3.Distance(ballPos, mousePos);
                if (dist <= mainscript.Instance.BallRealRadius)
                {
                    currentColor = ball.color;
                    findColor = true;
                    break;
                }                
            }
        }
        if (findColor == false)
            currentColor = 0;
        // TODO: 将与之重合的色球闪啊闪 在mainscript.cs里加一个闪啊闪的函数
    }
}

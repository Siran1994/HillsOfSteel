using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreeToTry : MonoBehaviour
{
    public Image TankImg;
    public Sprite[] TanSprites;
    public Text tankNameText;
    private int tankIndex=0;

    public Button[] TankS;
    void Start()
    {
        TankImg.sprite = TanSprites[PlayerPrefs.GetInt("TanIndex", 0)];

        TankS[0].onClick.AddListener(delegate//向左
        {
            tankIndex--;
            if (tankIndex<0)
            {
                tankIndex = 7;
            }

            PlayerPrefs.SetInt("TanIndex", tankIndex);
            TankImg.sprite = TanSprites[PlayerPrefs.GetInt("TanIndex")];
        });
        TankS[1].onClick.AddListener(delegate//向右
        {
            tankIndex++;
            if (tankIndex > 7)
            {
                tankIndex = 0;
            }
            PlayerPrefs.SetInt("TanIndex", tankIndex);
            TankImg.sprite = TanSprites[PlayerPrefs.GetInt("TanIndex")];
        });
        TankS[2].onClick.AddListener(delegate//退出
        {
            this.gameObject.SetActive(false);
        });
    }
    private void Update()
    {
        TanKName(PlayerPrefs.GetInt("TanIndex", 0));
    }
    void TanKName(int Index)
    {
        switch (Index)
        {
            case 0:
                tankNameText.text = "小丑";
                break;
            case 1:
                tankNameText.text = "泰坦";
                break;
            case 2:
                tankNameText.text = "凤凰";
                break;
            case 3:
                tankNameText.text = "收割者";
                break;
            case 4:
                tankNameText.text = "梭子鱼";
                break;
            case 5:
                tankNameText.text = "特斯拉";
                break;
            case 6:
                tankNameText.text = "猛犸象";
                break;
            case 7:
                tankNameText.text = "阿拉奇诺";
                break;
        }


    }

}

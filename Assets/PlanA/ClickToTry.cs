using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickToTry : MonoBehaviour
{
    public Transform Select;

    public Button[] TankS;
    public static bool Is6Selected = false;
    public static bool Is7Selected = false;
    public static bool Is8Selected = false;
    void Start()
    {
        TankS[0].onClick.AddListener(delegate
        {
            Select.transform.SetParent(TankS[0].transform);
            Select.transform.position = TankS[0].transform.position;
            Is6Selected = true;
            Is7Selected = false;
            Is8Selected = false;
        });
        TankS[1].onClick.AddListener(delegate
        {
            Select.transform.SetParent(TankS[1].transform);
            Select.transform.position = TankS[1].transform.position;
            Is6Selected = false;
            Is7Selected = true;
            Is8Selected = false;
        });
        TankS[2].onClick.AddListener(delegate
        {
            Select.transform.SetParent(TankS[2].transform);
            Select.transform.position = TankS[2].transform.position;
            Is6Selected = false;
            Is7Selected = false;
            Is8Selected = true;
        });
        TankS[3].onClick.AddListener(delegate
        {
            this.gameObject.SetActive(false);
        });
    }

   
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserPrivacy : MonoBehaviour
{
    Button Agree; //同意
    Button DisAgree;//不同意
    Button UserAgreement;//用户协议
    Button PrivacyPolicy;//隐私政策


    public GameObject UA_Panel;
    public GameObject PP_Panel;
    void Start()
    {
        PlayerPrefs.GetInt("IsAgree", 0);

        if (PlayerPrefs.GetInt("IsAgree") == 0)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
        Agree = transform.Find("Agree").GetComponent<Button>();
        Agree.onClick.AddListener(delegate 
        {
            this.gameObject.SetActive(false);
            PlayerPrefs.SetInt("IsAgree", 1);
        });
        DisAgree = transform.Find("DisAgree").GetComponent<Button>();
        DisAgree.onClick.AddListener(delegate
        {
            Application.Quit();
        });
        UserAgreement = transform.Find("UserAgreement").GetComponent<Button>();
        UserAgreement.onClick.AddListener(delegate
        {
            UA_Panel.SetActive(true);
           // Application.OpenURL("http://120.78.187.154:8001/privacy/user_agreement_szhy.html");           
        });
        PrivacyPolicy = transform.Find("PrivacyPolicy").GetComponent<Button>();
        PrivacyPolicy.onClick.AddListener(delegate
        {
            PP_Panel.SetActive(true);
            // Application.OpenURL("http://120.78.187.154:8001/privacy/privacy_szhy.html");
        });
    }

    
}

using UnityEngine;
using UnityEngine.UI;

public class PurchaseRestorePopup : MenuBase<PurchaseRestorePopup>
{
	public Button okButton;

	public GameObject failedContainer;

	public GameObject succeededContainer;

	private void OnEnable()
    {
        SetSucceeded();

    }

	private void Start()
	{
		okButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<PurchaseRestorePopup>();
            Time.timeScale = 1;
        });
	}

	public void SetFailed()
	{
		failedContainer.SetActive(value: true);
		succeededContainer.SetActive(value: false);
	}

	public void SetSucceeded()
	{
		failedContainer.SetActive(value: false);
		succeededContainer.SetActive(value: true);
	}
}

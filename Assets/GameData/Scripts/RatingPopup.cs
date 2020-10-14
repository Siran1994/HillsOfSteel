using UnityEngine.UI;

public class RatingPopup : MenuBase<RatingPopup>
{
	public Button ratingYesButton;

	public Button ratingNoButton;

	private void Start()
	{
		//ratingYesButton.onClick.AddListener(delegate
		//{
		//	MenuController.ShowMenu<AndroidRatingPopup>();
		//	MenuController.HideMenu<RatingPopup>();
		//});
		//ratingNoButton.onClick.AddListener(delegate
		//{
		//	MenuController.ShowMenu<CancelRatingPopup>();
		//	MenuController.HideMenu<RatingPopup>();
		//});
		//PlayerDataManager.SetRatingAsked();
	}
}

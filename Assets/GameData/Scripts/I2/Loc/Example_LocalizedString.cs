using UnityEngine;

namespace I2.Loc
{
	public class Example_LocalizedString : MonoBehaviour
	{
		public LocalizedString _MyLocalizedString;

		public string _NormalString;

		[TermsPopup]
		public string _StringWithTermPopup;

		private void Start()
		{
			_MyLocalizedString = "Term1";
			UnityEngine.Debug.Log(_MyLocalizedString);
			_MyLocalizedString = "Term2";
			UnityEngine.Debug.Log((string)_MyLocalizedString);
		}
	}
}

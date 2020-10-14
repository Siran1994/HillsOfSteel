using I2.Loc;
using UnityEngine;

public class TankLocalizationParams : MonoBehaviour, ILocalizationParamsManager
{
	private void OnEnable()
	{
		if (!LocalizationManager.ParamManagers.Contains(this))
		{
			LocalizationManager.ParamManagers.Add(this);
			LocalizationManager.LocalizeAll(Force: true);
		}
	}

	private void OnDisable()
	{
		LocalizationManager.ParamManagers.Remove(this);
	}

	public string GetParameterValue(string Param)
	{
		if (Param.Equals("SOCIAL_PLATFORM"))
		{
			return "Google Play Games";
		}
		return "null";
	}
}

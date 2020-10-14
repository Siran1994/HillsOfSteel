using I2.Loc;
using UnityEngine;

public class TankSetLanguage : MonoBehaviour
{
	public void ApplyLanguageAndQuit()
	{
		GetComponent<SetLanguage>().ApplyLanguage();
	}
}

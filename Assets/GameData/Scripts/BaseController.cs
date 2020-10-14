using System.Collections;
using UnityEngine;

public class BaseController : MonoBehaviour
{
	public delegate void Move(float val);

	public delegate GameObject Shoot(bool val);

	public delegate IEnumerator UseBooster();

	public Move OnMove;

	public Shoot OnShoot;

	public UseBooster OnUseBooster;

	public void Unregister()
	{
		OnMove = null;
		OnShoot = null;
		OnUseBooster = null;
	}

	public virtual void SetBooster(Booster booster)
	{
	}

	protected void DoMove(float v)
	{
		if (OnMove != null)
		{
			OnMove(v);
		}
	}

	protected void DoShoot(bool v)
	{
		if (OnShoot != null)
		{
			OnShoot(v);
		}
	}

	protected void DoBooster()
	{
		if (OnUseBooster != null)
		{
			StartCoroutine(OnUseBooster());
		}
	}
}

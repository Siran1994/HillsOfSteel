using UnityEngine;

public class PlayOnCollision : MonoBehaviour
{
	private static float timeLimit = 0.5f;

	public string audioName;

	private float nextTime;

	public void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (base.enabled && nextTime <= Time.time && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
		{
			AudioMap.PlayClipAt(TankGame.instance.audioMap, audioName, base.transform.position, TankGame.instance.audioMap.effectsMixerGroup);
			nextTime = Time.time + timeLimit;
		}
	}
}

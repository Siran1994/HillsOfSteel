using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
	public bool rounded;

	public Button button;

	public GameObject[] setActiveWhenDefault;

	public GameObject[] setActiveWhenToggled;

	public GameObject[] setActiveWhenDisabled;

	public CanvasGroup lowerOpacityWhenDisabled;

	[Range(0f, 1f)]
	public float alphaWhenDisabled;

	private ButtonState currentState;

	private ButtonSprites buttonSprites;

	private Action defaultCallback;

	private Action toggledCallback;

	private void Awake()
	{
		buttonSprites = (buttonSprites ?? (rounded ? Variables.instance.roundedButtonSprites : Variables.instance.defaultButtonSprites));
	}

	private void Start()
	{
		button = (button ?? GetComponent<Button>());
		button.onClick.AddListener(OnClick);
		ColorBlock colors = button.colors;
		colors.disabledColor = Color.white;
		button.colors = colors;
	}

	private void OnClick()
	{
		switch (currentState)
		{
		case ButtonState.Default:
			defaultCallback?.Invoke();
			break;
		case ButtonState.Toggled:
			toggledCallback?.Invoke();
			break;
		}
	}

	public void SetOnClick(ButtonState state, Action callback)
	{
		switch (state)
		{
		case ButtonState.Default:
			defaultCallback = callback;
			break;
		case ButtonState.Toggled:
			toggledCallback = callback;
			break;
		}
	}

	public void SetToggled(bool toggled)
	{
		Awake();
		currentState = (toggled ? ButtonState.Toggled : ButtonState.Default);
		button.image.sprite = (toggled ? buttonSprites.toggledState : buttonSprites.defaultState);
		button.interactable = true;
		SetActiveAll(setActiveWhenDisabled, active: false);
		SetActiveAll(setActiveWhenDefault, !toggled);
		SetActiveAll(setActiveWhenToggled, toggled);
		if ((bool)lowerOpacityWhenDisabled)
		{
			lowerOpacityWhenDisabled.alpha = 1f;
		}
	}

	public void SetDisabled()
	{
		currentState = ButtonState.Disabled;
		button.image.sprite = buttonSprites.disabledState;
		button.interactable = false;
		SetActiveAll(setActiveWhenDefault, active: false);
		SetActiveAll(setActiveWhenToggled, active: false);
		SetActiveAll(setActiveWhenDisabled, active: true);
		if ((bool)lowerOpacityWhenDisabled)
		{
			lowerOpacityWhenDisabled.alpha = alphaWhenDisabled;
		}
	}

	private void SetActiveAll(GameObject[] array, bool active)
	{
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i != array.Length; i++)
			{
				array[i].SetActive(active);
			}
		}
	}
}

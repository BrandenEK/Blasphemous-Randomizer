using System;
using Rewired;
using UnityEngine;

public class KeyDisplayer : MonoBehaviour
{
	private void Start()
	{
		ReInput.players.GetPlayer(0).AddInputEventDelegate(new Action<InputActionEventData>(this.ButtonPressed), 0, 3);
	}

	private void ButtonPressed(InputActionEventData obj)
	{
	}
}

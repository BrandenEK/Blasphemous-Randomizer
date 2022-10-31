using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

public class PenitentStillFrameEffect : SimpleVFX
{
	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		base.SetOrientationByPlayer();
		Sprite sprite = Core.Logic.Penitent.SpriteRenderer.sprite;
		if (this._spriteRenderers.Length == 0)
		{
			return;
		}
		this._spriteRenderers[0].sprite = sprite;
		base.transform.localScale = Vector3.one;
		base.transform.localPosition += Vector3.down * 0.07f;
		base.transform.DOPunchScale(Vector3.one * 0.15f, this.duration, 1, 0f).SetEase(Ease.InOutBack);
		this._spriteRenderers[0].color = new Color(this._spriteRenderers[0].color.r, this._spriteRenderers[0].color.g, this._spriteRenderers[0].color.b, 0.3f);
		this._spriteRenderers[0].DOFade(0f, this.duration);
	}

	public float duration = 0.3f;
}
using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using UnityEngine;

public class SquashStretchInOutEffect : MonoBehaviour
{
	private void Start()
	{
		this.shaderEffects = base.GetComponent<MasterShaderEffects>();
		this.sRenderer = base.GetComponent<SpriteRenderer>();
		if (this.instantiateOnDissappear != null)
		{
			PoolManager.Instance.CreatePool(this.instantiateOnDissappear, 1);
		}
	}

	public void Dissappear()
	{
		this.shaderEffects.TriggerColorizeLerp(0f, 0.75f, this.colorizeSeconds, new Action(this.SquashStretchOut));
		this.shaderEffects.TriggerColorTintLerp(0f, 1f, this.colorizeSeconds, null);
	}

	private void SquashStretchOut()
	{
		base.transform.DOScaleX(this.squashScaleXY.x, this.scaleSeconds).SetEase(Ease.InBack);
		base.transform.DOScaleY(this.squashScaleXY.y, this.scaleSeconds).SetEase(Ease.InBack).OnComplete(delegate
		{
			this.sRenderer.DOFade(0f, this.fadeSeconds);
		});
		this.shaderEffects.TriggerColorizeLerp(0.75f, 0f, this.colorizeSeconds, null);
		if (this.instantiateOnDissappear != null)
		{
			PoolManager.Instance.ReuseObject(this.instantiateOnDissappear, base.transform.position + this.effectOffset, Quaternion.identity, false, 1);
		}
	}

	private void SquashStretchIn()
	{
		base.transform.localScale = new Vector3(this.squashScaleXY.x, this.squashScaleXY.y, 1f);
		this.sRenderer.color = Color.white;
		this.sRenderer.DOFade(1f, this.fadeSeconds);
		base.transform.DOScaleX(1f, this.scaleSeconds).SetEase(Ease.InBack);
		base.transform.DOScaleY(1f, this.scaleSeconds).SetEase(Ease.InBack);
	}

	private void OnDrawGizmosSelected()
	{
		if (this.effectOffset != Vector2.zero)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(base.transform.position + this.effectOffset, 0.1f);
		}
	}

	private MasterShaderEffects shaderEffects;

	private SpriteRenderer sRenderer;

	public GameObject instantiateOnDissappear;

	public Vector2 effectOffset;

	public float colorizeSeconds = 0.2f;

	public float scaleSeconds = 0.3f;

	public float fadeSeconds = 0.4f;

	public Vector2 squashScaleXY;
}

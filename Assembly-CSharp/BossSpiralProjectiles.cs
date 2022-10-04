using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Projectiles;
using Sirenix.OdinInspector;
using UnityEngine;

public class BossSpiralProjectiles : MonoBehaviour
{
	private void Start()
	{
		this.dummies = new List<Transform>();
		this.currentProjectiles = new List<GameObject>();
		for (int i = 0; i < 20; i++)
		{
			GameObject gameObject = new GameObject(string.Format("SpinningDummy_{0}", i));
			gameObject.transform.SetParent(this.spinningTransform);
			this.dummies.Add(gameObject.transform);
			gameObject.SetActive(false);
		}
		PoolManager.Instance.CreatePool(this.projectilePrefab, 20);
	}

	[Button("TEST ATTACK", ButtonSizes.Small)]
	private void TestAttack()
	{
		this.ActivateAttack(this.testAttackNumber, this.testAttackDuration, this.testExtensionTime);
	}

	public void ActivateAttack(int numberOfProjectiles, float atkDuration, float extensionTime)
	{
		this.atkDuration = atkDuration;
		this.extensionTime = extensionTime;
		this.numberOfProjectiles = numberOfProjectiles;
		this.spinningTransform.rotation = Quaternion.identity;
		if (this.speedTween != null)
		{
			this.speedTween.Kill(false);
		}
		if (this.radiusTween != null)
		{
			this.radiusTween.Kill(false);
		}
		this.PrepareDummies();
		this.SetRadius(this.initialRadius);
		this.SetAngularSpeed(0f);
		this.SetTweens();
	}

	private void SetTweens()
	{
		float duration = this.atkDuration * 0.5f;
		this.radiusTween = DOTween.To(new DOGetter<float>(this.GetCurrentRadius), new DOSetter<float>(this.SetRadius), this.finalRadius, this.atkDuration).SetEase(this.radiusGrowthEase).SetUpdate(UpdateType.Normal, false);
		this.radiusTween.OnComplete(delegate
		{
			this.KeepRotating(this.extensionTime);
		});
		this.speedTween = DOTween.To(new DOGetter<float>(this.GetAngularSpeed), new DOSetter<float>(this.SetAngularSpeed), this.maxAngularSpeed, duration).SetUpdate(UpdateType.Normal, false).SetEase(this.angularSpeedEase);
	}

	private void KeepRotating(float v)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.SetUpdate(UpdateType.Normal, false);
		sequence.AppendInterval(0.016666668f);
		sequence.OnStepComplete(delegate
		{
			this.UpdateAllDummies(this.finalRadius);
		});
		sequence.SetLoops((int)(60f * v));
		sequence.Play<Sequence>();
	}

	private float GetAngularSpeed()
	{
		return this.spinningTransform.GetComponent<SpinBehavior>().angularSpeed;
	}

	private void SetAngularSpeed(float spd)
	{
		this.spinningTransform.GetComponent<SpinBehavior>().angularSpeed = spd;
	}

	private float GetCurrentRadius()
	{
		return this.currentRadius;
	}

	private void SetRadius(float newRadius)
	{
		this.currentRadius = newRadius;
		this.UpdateAllDummies(newRadius);
	}

	private void UpdateAllDummies(float newRadius)
	{
		for (int i = 0; i < this.numberOfProjectiles; i++)
		{
			this.UpdateDummies(i, this.currentRadius);
		}
	}

	private void PrepareDummies()
	{
		this.dummies.ForEach(delegate(Transform x)
		{
			x.gameObject.SetActive(false);
		});
		this.currentProjectiles.Clear();
		for (int i = 0; i < this.numberOfProjectiles; i++)
		{
			Quaternion rotation = Quaternion.Euler(0f, 0f, (float)(i * 360 / this.numberOfProjectiles));
			this.dummies[i].localPosition = rotation * Vector2.right * this.initialRadius;
			this.dummies[i].gameObject.SetActive(true);
			GameObject gameObject = PoolManager.Instance.ReuseObject(this.projectilePrefab, this.dummies[i].transform.position, Quaternion.identity, false, 1).GameObject;
			Projectile component = gameObject.GetComponent<Projectile>();
			component.timeToLive = this.atkDuration + this.extensionTime;
			component.ResetTTL();
			this.currentProjectiles.Add(gameObject);
		}
	}

	private void UpdateDummies(int index, float distance)
	{
		this.dummies[index].localPosition = this.dummies[index].localPosition.normalized * distance;
		this.currentProjectiles[index].transform.position = this.dummies[index].transform.position;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(this.spinningTransform.position, this.initialRadius);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(this.spinningTransform.position, this.finalRadius);
		if (this.dummies != null)
		{
			for (int i = 0; i < this.dummies.Count; i++)
			{
				if (this.dummies[i].gameObject.activeInHierarchy)
				{
					Gizmos.DrawWireSphere(this.dummies[i].position, 0.5f);
				}
			}
		}
	}

	[Header("Design")]
	public float initialRadius = 3f;

	public float finalRadius = 20f;

	public float maxAngularSpeed = 180f;

	public float atkDuration = 4f;

	public float extensionTime = 1f;

	public Ease radiusGrowthEase;

	public Ease angularSpeedEase;

	[Header("References")]
	public Transform spinningTransform;

	public GameObject projectilePrefab;

	private float currentRadius;

	private List<Transform> dummies;

	private List<GameObject> currentProjectiles;

	private int numberOfProjectiles;

	private const int MAX_PROJECTILES = 20;

	[Header("Debug")]
	public int testAttackNumber = 8;

	public float testAttackDuration = 2f;

	public float testExtensionTime = 2f;

	private Tween radiusTween;

	private Tween speedTween;
}

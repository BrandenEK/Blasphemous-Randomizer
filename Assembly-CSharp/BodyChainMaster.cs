using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class BodyChainMaster : MonoBehaviour
{
	[FoldoutGroup("Configuration", 0)]
	[Button("Auto assign links", ButtonSizes.Small)]
	public void AssignLinks()
	{
		for (int i = this.links.Count - 1; i >= 0; i--)
		{
			if (i < this.links.Count - 1)
			{
				this.links[i].previousLink = this.links[i + 1].transform.GetChild(1);
			}
			if (i > 0)
			{
				this.links[i].targetLink = this.links[i - 1].transform.GetChild(0);
				this.links[i].syncOffset = (float)(i + 1) * this.secAnimationStep;
			}
			else
			{
				this.links[i].targetLink = base.transform.GetChild(0);
			}
		}
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update chain", ButtonSizes.Small)]
	public void UpdateChain()
	{
		for (int i = 0; i < this.links.Count; i++)
		{
			this.links[i].UpdateChainLink();
		}
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update BACKWARDS chain", ButtonSizes.Small)]
	public void UpdateBackwardsChain()
	{
		for (int i = this.links.Count - 2; i >= 0; i--)
		{
			this.links[i].UpdateBackwardsChainLink();
		}
		base.transform.position = this.links[0].transform.GetChild(1).position;
		base.transform.rotation = this.links[0].transform.rotation;
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update reverse chain", ButtonSizes.Small)]
	public void UpdateReverseChain()
	{
		for (int i = this.links.Count - 1; i >= 0; i--)
		{
			this.links[i].ReverseUpdateChainLink();
			if (i > 1)
			{
				Vector3 right = this.links[i].transform.right;
				this.links[i - 1].baseAngle = 57.29578f * Mathf.Atan2(right.y, right.x);
			}
		}
	}

	public void Repullo()
	{
		this.InterruptActions();
		Vector3 a = base.transform.position - Core.Logic.Penitent.transform.position;
		a += Vector3.up;
		float d = 5f;
		this.MoveWithEase(base.transform.position + a.normalized * d, 0.5f, Ease.OutBack, null);
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update chain fixed point", ButtonSizes.Small)]
	public void UpdateFixedPoint()
	{
		for (int i = this.links.Count - 1; i >= 0; i--)
		{
			float num = this.links[i].DistanceToPoint();
			float d = num - this.links[i].distance;
			if (num > 0f)
			{
				Vector2 v = (this.links[i].targetLink.position - this.links[i].transform.position).normalized * d;
				for (int j = 0; j < i; j++)
				{
					this.links[j].transform.position -= v;
				}
				base.transform.position -= v;
			}
		}
	}

	[FoldoutGroup("Debug", 0)]
	[Button("TEST INTRO ENTRY", ButtonSizes.Small)]
	public void TestEntry()
	{
		SplineFollower component = base.GetComponent<SplineFollower>();
		component.followActivated = true;
		component.OnMovingToNextPoint += this.OnNextPointOnSight;
		component.OnMovementCompleted += this.OnMovementCompleted;
	}

	private void Start()
	{
		PoolManager.Instance.CreatePool(this.explosionFX, this.links.Count + 1);
		this.SecondaryAnimation();
	}

	private void SecondaryAnimation()
	{
		this.spr.transform.DOLocalMoveY(this.secAnimationAmplitude, this.secAnimationDuration, false).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
	}

	private void OnNextPointOnSight(Vector2 obj)
	{
		Vector2 vector = obj - base.transform.position;
		base.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector.y, vector.x) * 57.29578f);
	}

	private void OnMovementCompleted()
	{
		SplineFollower component = base.GetComponent<SplineFollower>();
		component.OnMovingToNextPoint -= this.OnNextPointOnSight;
		component.OnMovementCompleted -= this.OnMovementCompleted;
		int num = 5;
		for (int i = num; i < this.links.Count; i++)
		{
			this.links[i].Freeze(true);
		}
	}

	public void FlipAllSprites(bool flip)
	{
		foreach (BodyChainLink bodyChainLink in this.links)
		{
			bodyChainLink.FlipSprite(flip);
		}
		this.FlipSprite(flip);
	}

	public void LookRight()
	{
		this.FlipAllSprites(false);
	}

	public void LookLeft()
	{
		this.FlipAllSprites(true);
	}

	private void FlipSprite(bool flip)
	{
		this.spr.flipY = flip;
	}

	private void Update()
	{
		BodyChainMaster.CHAIN_UPDATE_MODES chain_UPDATE_MODES = this.chainMode;
		if (chain_UPDATE_MODES != BodyChainMaster.CHAIN_UPDATE_MODES.NORMAL)
		{
			if (chain_UPDATE_MODES == BodyChainMaster.CHAIN_UPDATE_MODES.BACKWARDS)
			{
				this.UpdateBackwardsChain();
			}
		}
		else
		{
			this.UpdateChain();
			this.UpdateFixedPoint();
		}
	}

	public void StartBob()
	{
		Vector2 v = base.transform.position + Vector2.up * 3f;
		base.transform.DOMove(v, 0.5f, false).SetLoops(5, LoopType.Yoyo).SetEase(Ease.InOutQuad);
	}

	public void EndBob()
	{
		base.transform.DOKill(false);
	}

	public List<Vector2> GeneratePointsAroundPoint(Vector2 p, float r, int n)
	{
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < n; i++)
		{
			list.Add(this.GenerateRandomPointInRadius(p, r));
		}
		return list;
	}

	private Vector2 GenerateRandomPointInRadius(Vector2 center, float r)
	{
		return center + new Vector2(UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r));
	}

	public Tween MoveWithEase(Vector2 targetPoint, float duration, Ease easingCurve, Action<Transform> callback = null)
	{
		return base.transform.DOMove(targetPoint, duration, false).SetEase(easingCurve).OnComplete(delegate
		{
			if (callback != null)
			{
				callback(this.transform);
			}
		});
	}

	public void SnakeAttack(Vector2 offset, Action callback = null)
	{
		base.StartCoroutine(this.SnakeAttackSequence(offset, callback));
	}

	private float GetMaxRange()
	{
		float num = 0f;
		for (int i = 0; i < this.links.Count; i++)
		{
			num += this.links[i].distance * 2f;
			if (this.links[i].fixedPoint)
			{
				break;
			}
		}
		return num + 1f;
	}

	private Vector3 GetCoilPos()
	{
		for (int i = 0; i < this.links.Count; i++)
		{
			if (this.links[i].fixedPoint)
			{
				return this.links[i].transform.position;
			}
		}
		return this.links[this.links.Count - 1].transform.position;
	}

	private IEnumerator SnakeAttackSequence(Vector2 offset, Action callbackBeforeAttack = null)
	{
		float anticipationTweenDuration = 0.4f;
		this.IsAttacking = true;
		float maxRange = this.GetMaxRange();
		maxRange = Mathf.Min(maxRange, this.maxAttackDistance);
		Vector2 origin = base.transform.position;
		Vector3 dir = Core.Logic.Penitent.transform.position + offset - origin;
		Tween curTween = base.transform.DOMove(base.transform.position - dir.normalized, anticipationTweenDuration, false).SetEase(Ease.OutCubic);
		yield return curTween.WaitForCompletion();
		if (callbackBeforeAttack != null)
		{
			callbackBeforeAttack();
		}
		curTween = base.transform.DOPunchPosition(dir.normalized * maxRange, this.attackDuration, 2, 0.1f, false);
		yield return curTween.WaitForCompletion();
		this.IsAttacking = false;
		yield break;
	}

	public void StartDeathSequence()
	{
		this.InterruptActions();
		base.StartCoroutine(this.DeathSequence());
	}

	private IEnumerator DeathSequence()
	{
		yield return new WaitForSeconds(0.2f);
		PoolManager.Instance.ReuseObject(this.explosionFX, base.transform.position, Quaternion.identity, false, 1);
		base.transform.GetComponentInChildren<SpriteRenderer>().enabled = false;
		yield return new WaitForSeconds(0.2f);
		for (int i = 0; i < this.links.Count; i++)
		{
			PoolManager.Instance.ReuseObject(this.explosionFX, this.links[i].transform.position, Quaternion.identity, false, 1);
			this.links[i].GetComponentInChildren<SpriteRenderer>().enabled = false;
			this.links[i].GetComponentInChildren<Collider2D>().enabled = false;
			Core.Logic.CameraManager.ProCamera2DShake.Shake(0.2f, Vector3.down * 2f, 4, 0.2f, 0f, default(Vector3), 0.1f, false);
			yield return new WaitForSeconds(0.2f);
		}
		this.DestroyAll();
		yield break;
	}

	private void DestroyAll()
	{
		for (int i = 0; i < this.links.Count; i++)
		{
			UnityEngine.Object.Destroy(this.links[i].gameObject);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void InterruptActions()
	{
		base.StopAllCoroutines();
		base.transform.DOKill(false);
		this.IsAttacking = false;
	}

	public void LookAtTarget(Vector3 point, float rotationSpeedFactor = 5f)
	{
		Vector2 vector = point - base.transform.position;
		Quaternion b = Quaternion.Euler(0f, 0f, 57.29578f * Mathf.Atan2(vector.y, vector.x));
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, rotationSpeedFactor * Time.deltaTime);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position + Vector3.up * this.bobPointDistance, this.bobPointsRadius);
		Gizmos.DrawWireSphere(base.transform.position + Vector3.down * this.bobPointDistance, this.bobPointsRadius);
		if (this.topPoints != null)
		{
			for (int i = 0; i < this.topPoints.Count; i++)
			{
				Gizmos.DrawWireSphere(this.topPoints[i], 0.05f);
			}
		}
		if (this.botPoints != null)
		{
			for (int j = 0; j < this.botPoints.Count; j++)
			{
				Gizmos.DrawWireSphere(this.botPoints[j], 0.05f);
			}
		}
	}

	public void AffixBody(bool affix, int affixIndex = 9)
	{
		this.links[affixIndex].fixedPoint = affix;
	}

	public void ForceStopAttack()
	{
		this.IsAttacking = false;
		base.StopAllCoroutines();
		DOTween.Kill(base.gameObject, false);
	}

	[FoldoutGroup("Configuration", 0)]
	public List<BodyChainLink> links;

	public BodyChainMaster.CHAIN_UPDATE_MODES chainMode;

	public float bobPointsRadius = 2f;

	public float bobPointDistance = 1.5f;

	public Ease bobEase;

	private Vector3 bobOrigin;

	private List<Vector2> topPoints;

	private List<Vector2> botPoints;

	private bool goUp;

	public float moveDuration = 0.5f;

	public bool IsStBobbing;

	public bool IsAttacking;

	public float secAnimationDuration = 3f;

	public float secAnimationAmplitude = 0.5f;

	public float secAnimationStep = 0.2f;

	public float maxAttackDistance = 10f;

	public float attackDuration = 1.4f;

	public SpriteRenderer spr;

	public GameObject explosionFX;

	public enum CHAIN_UPDATE_MODES
	{
		NORMAL,
		BACKWARDS
	}
}

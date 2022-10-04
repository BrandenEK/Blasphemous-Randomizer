using System;
using DG.Tweening;
using Framework.FrameworkCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities
{
	public class EntityDisplacement : Trait
	{
		private EntityMotionChecker MotionChecker { get; set; }

		protected override void OnAwake()
		{
			base.OnAwake();
			this.MotionChecker = base.EntityOwner.GetComponentInChildren<EntityMotionChecker>();
		}

		protected override void OnStart()
		{
			base.OnStart();
			base.EntityOwner.OnDamaged += this.OnDamaged;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (this.MotionChecker == null)
			{
				return;
			}
			if (this.MotionChecker.HitsBlock || !this.MotionChecker.HitsFloor)
			{
				if (DOTween.IsTweening("RegularDisplacement", false))
				{
					DOTween.Kill("RegularDisplacement", false);
				}
				if (DOTween.IsTweening("DamageDisplacement", false))
				{
					DOTween.Kill("DamageDisplacement", false);
				}
			}
		}

		private void OnDamaged()
		{
			if (!this.MotionChecker)
			{
				return;
			}
			if (this.MotionChecker.HitsBlock || !this.MotionChecker.HitsFloor)
			{
				return;
			}
			Hit lastHit = base.EntityOwner.EntityDamageArea.LastHit;
			Enemy enemy = (Enemy)base.EntityOwner;
			if (enemy && enemy.IsStunt)
			{
				return;
			}
			if (!base.EntityOwner.Status.Dead && this.OnHitDisplacement)
			{
				this.Move(lastHit);
			}
		}

		public void Move(float horDisplacement, float timeLapse, Ease ease)
		{
			Vector3 position = base.EntityOwner.transform.position;
			Vector2 entityPosition = new Vector2(position.x, position.y);
			horDisplacement = this.ClampHorizontalDisplacement(entityPosition, horDisplacement);
			Vector2 vector = new Vector2(entityPosition.x + horDisplacement, entityPosition.y);
			base.EntityOwner.transform.DOMoveX(vector.x, this.DisplacementTime, false).SetEase(ease).SetId("RegularDisplacement");
		}

		private void Move(Hit hit)
		{
			Vector3 position = base.EntityOwner.transform.position;
			float num = this.GetHorizontalDisplacement(hit);
			num = this.ClampHorizontalDisplacement(position, num);
			Vector2 vector = new Vector2(position.x + num, position.y);
			base.EntityOwner.transform.DOMoveX(vector.x, this.DisplacementTime, false).SetEase(this.DisplacementEase).SetId("DamageDisplacement");
		}

		private float ClampHorizontalDisplacement(Vector2 entityPosition, float distance)
		{
			float d = 0.2f;
			entityPosition += d * Vector2.up;
			Vector2 b = entityPosition + distance * Vector2.right;
			Vector2 b2;
			if (this.MotionChecker.HitsBlockInPosition(entityPosition, Mathf.Sign(distance) * Vector2.right, distance, out b2, true))
			{
				float f = Vector2.Distance(entityPosition, b2);
				return Mathf.Floor(f);
			}
			int num = 5;
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = Vector2.Lerp(entityPosition, b, (float)i / (float)num);
				if (!this.MotionChecker.HitsFloorInPosition(vector, 0.5f, out b2, true))
				{
					return Vector2.Distance(entityPosition, vector);
				}
			}
			return distance;
		}

		private float GetHorizontalDisplacement(Hit hit)
		{
			if (!hit.AttackingEntity)
			{
				return 0f;
			}
			Vector3 position = hit.AttackingEntity.transform.position;
			float displacementByDistance = this.GetDisplacementByDistance(base.EntityOwner.transform.position, position);
			float num = this.DisplacementByForce * hit.Force * displacementByDistance;
			if (position.x > base.EntityOwner.transform.position.x)
			{
				num *= -1f;
			}
			return num;
		}

		private float GetDisplacementByDistance(Vector2 a, Vector2 b)
		{
			float num = Vector2.Distance(a, b);
			num = Mathf.Clamp(num, this.DistanceFactor.x, this.DistanceFactor.y);
			return (this.DistanceFactor.y - num) / (this.DistanceFactor.y - this.DistanceFactor.x);
		}

		private void OnDestroy()
		{
			base.EntityOwner.OnDamaged -= this.OnDamaged;
		}

		private const string DamageDisplacement = "DamageDisplacement";

		private const string NormalDisplacement = "RegularDisplacement";

		[FoldoutGroup("Hit Displacement Settings", 0)]
		[Tooltip("Displacement in Unity Units by hit force.")]
		public float DisplacementByForce;

		[FoldoutGroup("Hit Displacement Settings", 0)]
		[Tooltip("Displacement movement ease.")]
		public AnimationCurve DisplacementEase;

		[FoldoutGroup("Hit Displacement Settings", 0)]
		[Tooltip("Time taken during displacement.")]
		public float DisplacementTime;

		[FoldoutGroup("Hit Displacement Settings", 0)]
		[Tooltip("Entity is displaced when is damaged")]
		public bool OnHitDisplacement = true;

		[MinMaxSlider(0f, 4f, false)]
		public Vector2 DistanceFactor;
	}
}

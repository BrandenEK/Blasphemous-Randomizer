using System;
using DG.Tweening;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Entities.Guardian.AI
{
	public class GuardianPrayerAttackState : State
	{
		public override void OnStateInitialize(StateMachine machine)
		{
			base.OnStateInitialize(machine);
			this._guardianPrayer = this.Machine.GetComponentInChildren<GuardianPrayer>();
		}

		public override void OnStateEnter()
		{
			base.OnStateEnter();
			this.ForwardMovement();
		}

		public override void OnStateExit()
		{
			base.OnStateExit();
		}

		public override void Update()
		{
			base.Update();
		}

		private void ForwardMovement()
		{
			float attackDistance = this._guardianPrayer.Behaviour.AttackDistance;
			float actionDirection = this._guardianPrayer.Behaviour.GetActionDirection(attackDistance);
			this._guardianPrayer.transform.DOMoveX(actionDirection, 0.2f, true).SetEase(Ease.InSine).OnStart(new TweenCallback(this.OnStartForwardMovement)).OnComplete(new TweenCallback(this.OnFinishForwardMovement));
		}

		private void Attack()
		{
			this._guardianPrayer.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.AttackTrigger);
			this._guardianPrayer.Audio.PlayAttack();
		}

		private void OnStartForwardMovement()
		{
			this.Attack();
		}

		private void OnFinishForwardMovement()
		{
		}

		private GuardianPrayer _guardianPrayer;
	}
}

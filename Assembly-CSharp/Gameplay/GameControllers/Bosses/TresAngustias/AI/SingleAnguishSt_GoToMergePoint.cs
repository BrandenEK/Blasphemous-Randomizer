using System;
using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.TresAngustias.AI
{
	public class SingleAnguishSt_GoToMergePoint : State<SingleAnguishBehaviour>
	{
		public override void Enter(SingleAnguishBehaviour owner)
		{
			owner.ActivateSteering(true);
			owner.ActivateGhostMode(true);
		}

		public override void Execute(SingleAnguishBehaviour owner)
		{
			owner.UpdateGoToTargetPoint();
			if (owner.IsCloseToTargetPoint(0.05f))
			{
				owner.NotifyMaster();
			}
		}

		public override void Exit(SingleAnguishBehaviour owner)
		{
			owner.ActivateSteering(false);
			owner.ActivateGhostMode(false);
		}
	}
}

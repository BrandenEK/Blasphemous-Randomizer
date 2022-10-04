using System;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action
{
	[ActionCategory("Blasphemous Action")]
	[Tooltip("Extend the maximum value of the fervour.")]
	public class FervourUpgrade : FsmStateAction
	{
		public override void OnEnter()
		{
			Core.Randomizer.giveReward(base.Owner.transform.position.GetHashCode().ToString(), true);
			base.Finish();
		}
	}
}

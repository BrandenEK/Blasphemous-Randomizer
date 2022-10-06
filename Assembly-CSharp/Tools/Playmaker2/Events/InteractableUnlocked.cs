using System;
using HutongGames.PlayMaker;
using Tools.Level;

namespace Tools.Playmaker2.Events
{
	[ActionCategory("Blasphemous Event")]
	[Tooltip("Event raised when an interactable is locked.")]
	public class InteractableUnlocked : FsmStateAction
	{
		public override void OnEnter()
		{
			if (this.listenOnlySelf.Value)
			{
				Interactable.SUnlocked += this.ListenToSelf;
			}
			else
			{
				Interactable.SUnlocked += this.ListenToAll;
			}
		}

		private void ListenToAll(Interactable go)
		{
			this.interactable.Value = go.gameObject;
			base.Fsm.Event(this.onSuccess);
			base.Finish();
		}

		private void ListenToSelf(Interactable go)
		{
			Interactable componentInChildren = base.Owner.GetComponentInChildren<Interactable>();
			if (go.Equals(componentInChildren))
			{
				this.interactable.Value = go.gameObject;
				base.Fsm.Event(this.onSuccess);
			}
			base.Finish();
		}

		public override void OnExit()
		{
			if (this.listenOnlySelf.Value)
			{
				Interactable.SUnlocked -= this.ListenToSelf;
			}
			else
			{
				Interactable.SUnlocked -= this.ListenToAll;
			}
		}

		[UIHint(10)]
		public FsmGameObject interactable;

		public FsmBool listenOnlySelf;

		public FsmEvent onSuccess;
	}
}

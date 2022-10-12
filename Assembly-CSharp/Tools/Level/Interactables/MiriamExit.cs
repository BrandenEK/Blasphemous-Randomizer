using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables
{
	public class MiriamExit : Interactable
	{
		public override bool IsIgnoringPersistence()
		{
			return true;
		}

		protected override void OnAwake()
		{
			LevelManager.OnLevelLoaded += this.OnLevelLoaded;
		}

		protected override void OnDispose()
		{
			LevelManager.OnLevelLoaded -= this.OnLevelLoaded;
		}

		private void OnLevelLoaded(Level oldLevel, Level newLevel)
		{
			this.IsUsing = false;
		}

		protected override void OnUpdate()
		{
			if (Core.Logic != null && Core.Logic.Penitent == null)
			{
				return;
			}
			if (!this.IsUsing && base.PlayerInRange && base.InteractionTriggered && !base.Locked && !Core.Logic.Penitent.IsClimbingLadder)
			{
				this.UsePortal();
			}
			if (base.BeingUsed)
			{
				this.PlayerReposition();
			}
		}

		protected override void InteractionEnd()
		{
			this.ShowPlayer(true);
		}

		private void UsePortal()
		{
			base.StartCoroutine(this.UseCourrutine());
		}

		private IEnumerator UseCourrutine()
		{
			if (this.RepositionBeforeInteract)
			{
				Core.Logic.Penitent.DrivePlayer.MoveToPosition(this.Waypoint.position, this.orientation);
				do
				{
					yield return null;
				}
				while (Core.Logic.Penitent.DrivePlayer.Casting);
			}
			this.IsUsing = true;
			this.ShowPlayer(false);
			Core.Input.SetBlocker("MIRIAM_PORTAL", true);
			Core.Logic.Penitent.Status.CastShadow = false;
			this.interactorAnimator.Play(this.InteractorAnimationName);
			yield return new WaitForSeconds(this.InteractableAnimationDelay);
			Core.Audio.PlayOneShot(this.SharkSound, default(Vector3));
			this.interactableAnimator.SetTrigger(this.InteractableAnimationTrigger);
			yield return new WaitForSeconds(this.FadeDelay);
			Core.Audio.StopNamedSound("TeleportAltarMiriam", FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			FadeWidget.OnFadeShowEnd += this.OnFadeShowEnd;
			FadeWidget.instance.Fade(true, this.FadeTime, 0f, null);
			yield break;
		}

		private void OnFadeShowEnd()
		{
			ProCamera2D.Instance.HorizontalFollowSmoothness = 0f;
			ProCamera2D.Instance.VerticalFollowSmoothness = 0f;
			FadeWidget.OnFadeShowEnd -= this.OnFadeShowEnd;
			Core.Input.SetBlocker("MIRIAM_PORTAL", false);
			Core.Events.EndMiriamPortalAndReturn(this.UseFade);
		}

		[BoxGroup("Design Settings", true, false, 0)]
		public bool UseFade = true;

		[BoxGroup("Design Settings", true, false, 0)]
		public string InteractorAnimationName = string.Empty;

		[BoxGroup("Design Settings", true, false, 0)]
		public string InteractableAnimationTrigger = "TRIGGER_TAKE";

		[BoxGroup("Design Settings", true, false, 0)]
		public float InteractableAnimationDelay = 1f;

		[BoxGroup("Design Settings", true, false, 0)]
		public float FadeDelay = 1f;

		[BoxGroup("Design Settings", true, false, 0)]
		public float FadeTime = 0.6f;

		[BoxGroup("Design Settings", true, false, 0)]
		[EventRef]
		public string SharkSound = string.Empty;

		private bool IsUsing;
	}
}

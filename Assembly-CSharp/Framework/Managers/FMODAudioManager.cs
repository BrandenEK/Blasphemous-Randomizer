using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DG.Tweening;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Framework.Audio;
using Framework.FrameworkCore;
using Framework.Util;
using Sirenix.Utilities;
using Tools.Audio;
using UnityEngine;

namespace Framework.Managers
{
	public class FMODAudioManager : GameSystem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event FMODAudioManager.ProgrammerSoundSeted OnProgrammerSoundSeted;

		public Bus Sfx { get; private set; }

		public Bus Music { get; private set; }

		public Bus Voiceover { get; private set; }

		public AmbientMusicSettings Ambient { get; private set; }

		public ChannelGroup SfxChannelGroup { get; private set; }

		public FMODAudioCatalog[] EnemiesAudioCatalogs { get; private set; }

		public override void Initialize()
		{
			base.Initialize();
			this.NamedSounds = new Dictionary<string, EventInstance>();
			this._audioCatalogs = new FMODAudioCatalog[0];
			Settings instance = Settings.Instance;
			if (instance.AutomaticEventLoading || instance.ImportType != null)
			{
				Debug.LogError("*** FMODAudioManager, setting must be AutomaticEventLoading=false and ImportType=StreamingAssets");
			}
			else
			{
				try
				{
					foreach (string text in instance.MasterBanks)
					{
						RuntimeManager.LoadBank(text + ".strings", instance.AutomaticSampleLoading);
						RuntimeManager.LoadBank(text, instance.AutomaticSampleLoading);
					}
					foreach (string text2 in instance.Banks)
					{
						if (!text2.ToUpper().StartsWith("VOICEOVER_"))
						{
							RuntimeManager.LoadBank(text2, instance.AutomaticSampleLoading);
						}
					}
					RuntimeManager.WaitForAllLoads();
				}
				catch (BankLoadException ex)
				{
					Debug.LogException(ex);
				}
			}
			this.Sfx = RuntimeManager.GetBus("bus:/ALLSFX");
			this.Music = RuntimeManager.GetBus("bus:/MUSIC");
			this.Voiceover = RuntimeManager.GetBus("bus:/VO");
			if (FMODAudioManager.<>f__mg$cache0 == null)
			{
				FMODAudioManager.<>f__mg$cache0 = new EVENT_CALLBACK(FMODAudioManager.ProgrammerSoundCallback);
			}
			this.programmerSoundCallback = FMODAudioManager.<>f__mg$cache0;
			this.Ambient = new AmbientMusicSettings();
			this.EnemiesAudioCatalogs = Resources.LoadAll<FMODAudioCatalog>("FMODCatalogs/Enemies");
			LocalizationManager.OnLocalizeAudioEvent += this.OnAudioLocalizationChange;
		}

		private void OnAudioLocalizationChange(string idlang)
		{
			this.LocalizationValue = ((!(idlang.ToUpper() == "ES")) ? 0f : 1f);
		}

		public override void Update()
		{
			if (this.Ambient != null)
			{
				this.Ambient.Update();
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, EventInstance> keyValuePair in this.NamedSounds)
			{
				EventInstance value = keyValuePair.Value;
				bool flag;
				if (!value.isValid())
				{
					flag = true;
				}
				else
				{
					PLAYBACK_STATE playback_STATE;
					value.getPlaybackState(ref playback_STATE);
					flag = (playback_STATE == 2 || playback_STATE == 4);
					value.release();
				}
				if (flag)
				{
					list.Add(keyValuePair.Key);
				}
			}
			list.ForEach(delegate(string element)
			{
				this.NamedSounds.Remove(element);
			});
		}

		public override void OnGUI()
		{
			this.Ambient.OnGUI();
			this.posYGUI = 10f;
			this.DrawTextLine("Named Sounds -------------------------------------");
			this.DrawTextLine("Total:" + this.NamedSounds.Count);
			this.DrawTextLine("----------------------");
			foreach (KeyValuePair<string, EventInstance> keyValuePair in this.NamedSounds)
			{
				string str = "NOT VALID";
				if (keyValuePair.Value.isValid())
				{
					PLAYBACK_STATE playback_STATE;
					keyValuePair.Value.getPlaybackState(ref playback_STATE);
					int num;
					keyValuePair.Value.getTimelinePosition(ref num);
					str = playback_STATE + "  Pos:" + num;
				}
				this.DrawTextLine(keyValuePair.Key + ": " + str);
			}
			this.DrawTextLine("Playing Sounds -------------------------------------");
			Bank[] array = null;
			RuntimeManager.StudioSystem.getBankList(ref array);
			foreach (Bank bank in array)
			{
				EventDescription[] array3 = null;
				string empty = string.Empty;
				bank.getEventList(ref array3);
				bank.getPath(ref empty);
				this.DrawTextLine("BANK:" + empty);
				foreach (EventDescription eventDescription in array3)
				{
					int num2 = 0;
					eventDescription.getInstanceCount(ref num2);
					if (num2 > 0)
					{
						string empty2 = string.Empty;
						eventDescription.getPath(ref empty2);
						this.DrawTextLine(string.Concat(new object[]
						{
							".",
							empty2,
							" (",
							num2,
							")"
						}));
					}
				}
			}
		}

		private void DrawTextLine(string text)
		{
			GUI.Label(new Rect(500f, this.posYGUI, 1000f, this.posYGUI + 10f), text);
			this.posYGUI += 13f;
		}

		public EventInstance PlayProgrammerSound(string eventName, string keyName, FMODAudioManager.ProgrammerSoundSeted eventSound)
		{
			FMODAudioManager.currentEvent = eventSound;
			EventInstance eventInstance = RuntimeManager.CreateInstance(eventName);
			GCHandle value = GCHandle.Alloc(keyName, GCHandleType.Pinned);
			eventInstance.setUserData(GCHandle.ToIntPtr(value));
			eventInstance.setCallback(this.programmerSoundCallback, -1);
			this.UpdateEventInstanceParam(eventInstance);
			eventInstance.start();
			eventInstance.release();
			return eventInstance;
		}

		private static RESULT ProgrammerSoundCallback(EVENT_CALLBACK_TYPE type, EventInstance eventInstance, IntPtr parameterPtr)
		{
			IntPtr value;
			eventInstance.getUserData(ref value);
			GCHandle gchandle = GCHandle.FromIntPtr(value);
			string text = gchandle.Target as string;
			if (type != 128)
			{
				if (type != 256)
				{
					if (type == 2)
					{
						gchandle.Free();
					}
				}
				else
				{
					PROGRAMMER_SOUND_PROPERTIES programmer_SOUND_PROPERTIES = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
					Sound sound = default(Sound);
					sound.handle = programmer_SOUND_PROPERTIES.sound;
					sound.release();
				}
			}
			else
			{
				PROGRAMMER_SOUND_PROPERTIES programmer_SOUND_PROPERTIES2 = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
				SOUND_INFO sound_INFO;
				RESULT soundInfo = RuntimeManager.StudioSystem.getSoundInfo(text, ref sound_INFO);
				if (soundInfo != null)
				{
					Debug.LogWarning(string.Concat(new object[]
					{
						"Programmer sound: Can't find key ",
						text,
						" ERR:",
						soundInfo
					}));
				}
				else
				{
					Sound sound2;
					RESULT result = RuntimeManager.LowlevelSystem.createSound(sound_INFO.name_or_data, sound_INFO.mode, ref sound_INFO.exinfo, ref sound2);
					if (result == null)
					{
						programmer_SOUND_PROPERTIES2.sound = sound2.handle;
						programmer_SOUND_PROPERTIES2.subsoundIndex = sound_INFO.subsoundindex;
						Marshal.StructureToPtr(programmer_SOUND_PROPERTIES2, parameterPtr, false);
						if (FMODAudioManager.currentEvent != null)
						{
							Sound sound3;
							sound2.getSubSound(sound_INFO.subsoundindex, ref sound3);
							uint num;
							sound3.getLength(ref num, 1);
							float time = num / 1000f;
							FMODAudioManager.currentEvent(time);
						}
					}
					else
					{
						Debug.LogWarning(string.Concat(new object[]
						{
							"Programmer sound: Can't create sound, key ",
							text,
							" ERR:",
							result
						}));
					}
				}
			}
			return 0;
		}

		public void PlayNamedSound(string eventName, string keyName)
		{
			EventInstance eventInstance = default(EventInstance);
			if (this.NamedSounds.ContainsKey(keyName))
			{
				eventInstance = this.NamedSounds[keyName];
			}
			if (!eventInstance.isValid())
			{
				eventInstance = RuntimeManager.CreateInstance(eventName);
				this.UpdateEventInstanceParam(eventInstance);
				this.NamedSounds[keyName] = eventInstance;
			}
			RESULT result = eventInstance.start();
		}

		public void StopNamedSound(string keyName, STOP_MODE stopMode = 0)
		{
			if (this.IsPlayingNamedSound(keyName))
			{
				this.NamedSounds[keyName].stop(stopMode);
				this.NamedSounds[keyName].release();
				this.NamedSounds.Remove(keyName);
			}
		}

		public bool IsPlayingNamedSound(string keyName)
		{
			bool result = false;
			if (this.NamedSounds.ContainsKey(keyName) && this.NamedSounds[keyName].isValid())
			{
				PLAYBACK_STATE playback_STATE;
				this.NamedSounds[keyName].getPlaybackState(ref playback_STATE);
				result = (playback_STATE == null || playback_STATE == 3);
			}
			return result;
		}

		private FMODAudioCatalog.FMODIndexedClip FindSfxInCatalogs(string id)
		{
			return (from t in this._audioCatalogs
			select t.GetSfx(id)).FirstOrDefault((FMODAudioCatalog.FMODIndexedClip c) => c != null);
		}

		private void UpdateEventInstanceParam(EventInstance evt)
		{
			if (!evt.isValid())
			{
				return;
			}
			evt.setParameterValue("Spanish", this.LocalizationValue);
		}

		public void RegisterCatalog(FMODAudioCatalog cat)
		{
			bool flag = this._audioCatalogs.Any((FMODAudioCatalog t) => t == cat);
			if (flag)
			{
				return;
			}
			FMODAudioCatalog[] array = new FMODAudioCatalog[this._audioCatalogs.Length + 1];
			this._audioCatalogs.CopyTo(array, 0);
			array[this._audioCatalogs.Length] = cat;
			this._audioCatalogs = array;
			cat.Initialize();
		}

		public void PlayOneShot(string path, Vector3 position = default(Vector3))
		{
			try
			{
				this.PlayOneShot(RuntimeManager.PathToGUID(path), position);
			}
			catch (EventNotFoundException)
			{
				Debug.LogWarning("[FMOD] Event not found: " + path);
			}
		}

		public void PlayOneShot(Guid guid, Vector3 position = default(Vector3))
		{
			EventInstance evt = RuntimeManager.CreateInstance(guid);
			evt.set3DAttributes(RuntimeUtils.To3DAttributes(position));
			this.UpdateEventInstanceParam(evt);
			evt.start();
			evt.release();
		}

		public EventInstance CreateCatalogEvent(string key, Vector3 position = default(Vector3))
		{
			FMODAudioCatalog.FMODIndexedClip fmodindexedClip = this.FindSfxInCatalogs(key);
			EventInstance eventInstance = (fmodindexedClip != null) ? RuntimeManager.CreateInstance(fmodindexedClip.FMODKey) : default(EventInstance);
			eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
			this.UpdateEventInstanceParam(eventInstance);
			return eventInstance;
		}

		public EventInstance CreateEvent(string key, Vector3 position = default(Vector3))
		{
			EventInstance eventInstance = default(EventInstance);
			if (!StringExtensions.IsNullOrWhitespace(key))
			{
				eventInstance = RuntimeManager.CreateInstance(key);
			}
			eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
			this.UpdateEventInstanceParam(eventInstance);
			if (!eventInstance.isValid())
			{
				Log.Error("Audio", "Imposible to create audio instance. ID: " + key, null);
			}
			return eventInstance;
		}

		public static EventInstance CloneInstance(EventDescription desc)
		{
			EventInstance result;
			desc.createInstance(ref result);
			return result;
		}

		public void EventOneShot(EventInstance eventInstance)
		{
			EventDescription desc;
			eventInstance.getDescription(ref desc);
			EventInstance eventInstance2 = FMODAudioManager.CloneInstance(desc);
			eventInstance2.start();
			eventInstance2.release();
		}

		public void EventOneShotPanned(string eventKey, Vector3 position)
		{
			if (StringExtensions.IsNullOrWhitespace(eventKey))
			{
				return;
			}
			EventInstance evt = RuntimeManager.CreateInstance(eventKey);
			this.UpdateEventInstanceParam(evt);
			ParameterInstance parameterInstance;
			evt.getParameter("Panning", ref parameterInstance);
			if (parameterInstance.isValid())
			{
				float panningValueByPosition = FMODAudioManager.GetPanningValueByPosition(position);
				parameterInstance.setValue(panningValueByPosition);
			}
			evt.start();
			evt.release();
		}

		public void PlayOneShotFromCatalog(string eventKey, Vector3 position)
		{
			FMODAudioCatalog.FMODIndexedClip fmodindexedClip = this.FindSfxInCatalogs(eventKey);
			if (fmodindexedClip == null)
			{
				return;
			}
			string fmodkey = fmodindexedClip.FMODKey;
			this.EventOneShotPanned(fmodkey, position);
		}

		public void EventOneShotPanned(string eventKey, Vector3 position, out EventInstance eventInstance)
		{
			if (StringExtensions.IsNullOrWhitespace(eventKey))
			{
				eventInstance = default(EventInstance);
				return;
			}
			eventInstance = RuntimeManager.CreateInstance(eventKey);
			this.UpdateEventInstanceParam(eventInstance);
			ParameterInstance parameterInstance;
			eventInstance.getParameter("Panning", ref parameterInstance);
			if (parameterInstance.isValid())
			{
				float panningValueByPosition = FMODAudioManager.GetPanningValueByPosition(position);
				parameterInstance.setValue(panningValueByPosition);
			}
			eventInstance.start();
			eventInstance.release();
		}

		public void PlaySfxOnCatalog(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return;
			}
			FMODAudioCatalog.FMODIndexedClip fmodindexedClip = this.FindSfxInCatalogs(id);
			if (fmodindexedClip != null)
			{
				string fmodkey = fmodindexedClip.FMODKey;
				RuntimeManager.PlayOneShot(fmodkey, default(Vector3));
			}
			else
			{
				Debug.LogError(string.Format("SFX {0} not defined in any AudioCatalog!", id));
			}
		}

		public void FadeAudio(float volume, float time)
		{
			DOTween.To(() => Core.Audio.MasterVolume, delegate(float x)
			{
				Core.Audio.MasterVolume = x;
			}, volume, time);
		}

		public void FadeLevelAudio(float volume, float time)
		{
			DOTween.To(() => Core.Audio.Ambient.Volume, delegate(float x)
			{
				Core.Audio.Ambient.Volume = x;
			}, volume, time);
		}

		public void PauseAudio(bool pause)
		{
			RuntimeManager.PauseAllEvents(pause);
		}

		public void PlaySfxOnCatalog(string id, float delay)
		{
			Singleton<Core>.Instance.StartCoroutine(this.PlaySfxDelay(id, delay));
		}

		public void PlaySfx(string id, float delay = 0f)
		{
			if (StringExtensions.IsNullOrWhitespace(id))
			{
				return;
			}
			TweenSettingsExtensions.OnComplete<Sequence>(TweenSettingsExtensions.SetDelay<Sequence>(DOTween.Sequence(), delay), delegate()
			{
				RuntimeManager.PlayOneShot(id, default(Vector3));
			});
		}

		public ChannelGroup GetMasterGroup()
		{
			ChannelGroup result;
			RuntimeManager.LowlevelSystem.getMasterChannelGroup(ref result);
			return result;
		}

		public EVENT_CALLBACK ModifyPanning(EventInstance e, Transform transform)
		{
			ParameterInstance parameterInstance;
			e.getParameter("Panning", ref parameterInstance);
			if (parameterInstance.isValid())
			{
				float panningValueByPosition = FMODAudioManager.GetPanningValueByPosition(transform.position);
				parameterInstance.setValue(panningValueByPosition);
			}
			return null;
		}

		public void ApplyDistanceParam(ref EventInstance ev, float minDist, float maxDist, Transform a, Transform b)
		{
			if (!ev.isValid())
			{
				return;
			}
			float distanceParam = this.GetDistanceParam(minDist, maxDist, a, b);
			ParameterInstance parameterInstance;
			ev.getParameter("Distance", ref parameterInstance);
			if (parameterInstance.isValid())
			{
				parameterInstance.setValue(distanceParam);
			}
		}

		public void PlayEventWithCatalog(ref EventInstance eventInstance, string eventKey, Vector3 position = default(Vector3))
		{
			if (eventInstance.isValid())
			{
				return;
			}
			eventInstance = this.CreateCatalogEvent(eventKey, position);
			eventInstance.start();
		}

		public void PlayEventNoCatalog(ref EventInstance eventInstance, string eventKey, Vector3 position = default(Vector3))
		{
			if (eventInstance.isValid())
			{
				return;
			}
			eventInstance = this.CreateEvent(eventKey, position);
			eventInstance.start();
		}

		public void StopEvent(ref EventInstance eventInstance)
		{
			if (!eventInstance.isValid())
			{
				return;
			}
			eventInstance.stop(0);
			eventInstance.release();
			eventInstance.clearHandle();
		}

		private float GetDistanceParam(float min, float max, Transform a, Transform b)
		{
			float num = Vector2.Distance(a.position, b.position);
			num = Mathf.Clamp(num, min, max);
			return (num - min) / (max - min);
		}

		public void PlaySfxAtPosition(string id, Vector2 position, float range)
		{
			if (StringExtensions.IsNullOrWhitespace(id))
			{
				return;
			}
			EventInstance eventInstance = RuntimeManager.CreateInstance(id);
			this.UpdateEventInstanceParam(eventInstance);
			Vector2 vector = Core.Logic.Penitent.transform.position;
			float num = Math.Max(0f, Vector2.Distance(vector, position));
			float volume = 1f - Mathf.Clamp01(num / range);
			eventInstance.setVolume(volume);
			eventInstance.start();
			eventInstance.setCallback(this.PlaySfxAtPositionFinished(eventInstance), 32);
		}

		private EVENT_CALLBACK PlaySfxAtPositionFinished(EventInstance instance)
		{
			instance.release();
			return null;
		}

		public void StopSfx(string id, bool allowFadeout = false)
		{
			FMODAudioCatalog.FMODIndexedClip fmodindexedClip = this.FindSfxInCatalogs(id);
			if (fmodindexedClip == null)
			{
				return;
			}
			EventInstance eventInstance = RuntimeManager.CreateInstance(fmodindexedClip.FMODKey);
			PLAYBACK_STATE playback_STATE;
			eventInstance.getPlaybackState(ref playback_STATE);
			if (playback_STATE == null || playback_STATE == 3)
			{
				eventInstance.stop((!allowFadeout) ? 1 : 0);
			}
			eventInstance.release();
		}

		private IEnumerator PlaySfxDelay(string id, float delay)
		{
			yield return new WaitForSeconds(delay);
			this.PlaySfxOnCatalog(id);
			yield break;
		}

		public static float GetPanningValueByPosition(Vector2 pos)
		{
			float num = Mathf.Clamp01(Camera.main.WorldToViewportPoint(pos).x);
			return Mathf.Lerp(0f, 1f, num);
		}

		public float MasterVolume
		{
			get
			{
				float result;
				this.GetMasterGroup().getVolume(ref result);
				return result;
			}
			set
			{
				this.GetMasterGroup().setVolume(value);
			}
		}

		public void SetSfxVolume(float volume)
		{
			RESULT result = this.Sfx.setVolume(Mathf.Clamp01(volume));
			if (result != null)
			{
				Debug.LogError("SetSfxVolume error! Operation result: " + result);
			}
		}

		public void SetMusicVolume(float volume)
		{
			RESULT result = this.Music.setVolume(Mathf.Clamp01(volume));
			if (result != null)
			{
				Debug.LogError("SetMusicVolume error! Operation result: " + result);
			}
		}

		public void SetVoiceoverVolume(float volume)
		{
			RESULT result = this.Voiceover.setVolume(Mathf.Clamp01(volume));
			if (result != null)
			{
				Debug.LogError("SetVoiceoverVolume error! Operation result: " + result);
			}
		}

		public float GetSfxVolume()
		{
			float result;
			float num;
			RESULT volume = this.Sfx.getVolume(ref result, ref num);
			if (volume != null)
			{
				Debug.LogError("GetSfxVolume error! Operation result: " + volume);
			}
			return result;
		}

		public float GetMusicVolume()
		{
			float result;
			float num;
			RESULT volume = this.Music.getVolume(ref result, ref num);
			if (volume != null)
			{
				Debug.LogError("GetMusicVolume error! Operation result: " + volume);
			}
			return result;
		}

		public float GetVoiceoverVolume()
		{
			float result;
			float num;
			RESULT volume = this.Voiceover.getVolume(ref result, ref num);
			if (volume != null)
			{
				Debug.LogError("GetVoiceoverVolume error! Operation result: " + volume);
			}
			return result;
		}

		public const float LeftPanValue = 0f;

		public const float RightPanValue = 1f;

		private static FMODAudioManager.ProgrammerSoundSeted currentEvent;

		private EVENT_CALLBACK programmerSoundCallback;

		private Dictionary<string, EventInstance> NamedSounds;

		private float posYGUI;

		private const string LocalizationParam = "Spanish";

		private float LocalizationValue;

		private FMODAudioCatalog[] _audioCatalogs;

		[CompilerGenerated]
		private static EVENT_CALLBACK <>f__mg$cache0;

		public delegate void ProgrammerSoundSeted(float time);
	}
}

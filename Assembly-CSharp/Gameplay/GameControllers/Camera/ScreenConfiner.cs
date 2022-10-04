using System;
using UnityEngine;

namespace Gameplay.GameControllers.Camera
{
	public class ScreenConfiner : MonoBehaviour
	{
		public void EnableBoundary()
		{
			Debug.Log("enable");
			if (this.levelLeftBoundary)
			{
				this.levelLeftBoundary.gameObject.GetComponent<Collider2D>().enabled = true;
			}
		}

		public void DisableBoundary()
		{
			if (this.levelLeftBoundary)
			{
				this.levelLeftBoundary.gameObject.GetComponent<Collider2D>().enabled = false;
			}
		}

		private void Start()
		{
			Vector2 v = new Vector2(this.cameraNumericBoundaries.LeftBoundary, this.cameraNumericBoundaries.TopBoundary / 2f);
			this.levelLeftBoundary = UnityEngine.Object.Instantiate<GameObject>(this.levelLeftBoundaryPrefab, v, Quaternion.identity);
			this.levelLeftBoundary.transform.SetParent(base.transform, true);
			this.DisableBoundary();
		}

		private void LateUpdate()
		{
			Vector2 v = new Vector2(this.cameraNumericBoundaries.LeftBoundary, this.cameraNumericBoundaries.TopBoundary / 2f);
			this.levelLeftBoundary.transform.position = v;
		}

		[SerializeField]
		private CameraNumericBoundaries cameraNumericBoundaries;

		[SerializeField]
		private GameObject levelLeftBoundaryPrefab;

		private GameObject levelLeftBoundary;
	}
}

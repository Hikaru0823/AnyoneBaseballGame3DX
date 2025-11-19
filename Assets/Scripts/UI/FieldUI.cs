using UnityEngine;

public class FieldUI : MonoBehaviour
{
		private Transform _cameraTransform;

		private void Awake()
		{
			_cameraTransform = Camera.main.transform;
		}

		private void LateUpdate()
		{
			// Rotate nameplate toward camera
			transform.rotation = _cameraTransform.rotation;
		}
	}

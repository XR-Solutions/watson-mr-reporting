using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ParentPosition : MonoBehaviour
{
	// Start is called before the first frame update
	private Transform ParentTransform;
	[SerializeField]
	private float CustomDistance = 4.0f;

	private Transform ChildTransform;

	private void Awake()
	{
		this.ChildTransform = this.GetComponent<Transform>();
		ParentTransform = Camera.main.transform;

		if (ParentTransform == null)
		{
			Debug.LogWarning("No camera transform found");
		}
	}

	public Vector3 GetParentSpawnPosition() 
	{
		// Get the camera's position and rotation
		Vector3 cameraPosition = Camera.main.transform.position;
		Quaternion cameraRotation = Camera.main.transform.rotation;

		// Calculate the spawn position based on camera rotation
		return cameraPosition + cameraRotation * Vector3.forward * CustomDistance;
	}

	public Quaternion GetParentRotation()
	{ 
		return ParentTransform.rotation;
	}

	public void MoveToParent()
	{
		if (this.ParentTransform == null || this.ChildTransform == null)
		{
			Debug.LogWarning("Cannot move when transforms are empty");
			return;
		}

		ChildTransform.position = ParentTransform.position;

		Debug.Log($"{ChildTransform}");
	}

	public void MoveInFrontOfParent()
	{
		if (this.ParentTransform == null || this.ChildTransform == null)
		{
			Debug.LogWarning("Cannot move when transforms are empty");
			return;
		}

		// Get the camera's position and rotation
		Vector3 cameraPosition = Camera.main.transform.position;
		Quaternion cameraRotation = Camera.main.transform.rotation;

		// Calculate the spawn position based on camera rotation
		Vector3 spawnPosition = cameraPosition + cameraRotation * Vector3.forward * CustomDistance;

		ChildTransform.position = spawnPosition;

		Debug.Log($"{ChildTransform}");
	}
}

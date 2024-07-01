using Microsoft.MixedReality.OpenXR;
using UnityEngine;

public class QRCodeBehaviour : MonoBehaviour
{
	public static Vector3 QRCodePosition { get; private set; }
	[SerializeField] public NoteSystem noteSystem;
	[SerializeField] private ARMarkerManager markerManager;

	private float lastUpdateTime;
	private float cooldownDuration = 10f; // 10 seconds cooldown

	private void Start()
	{
		if (markerManager == null)
		{
			Debug.LogError("ARMarkerManager is not assigned.");
			return;
		}

		if (noteSystem == null)
		{
			Debug.LogError("NoteSystem is not assigned.");
			return;
		}

		markerManager.markersChanged += OnMarkersChanged;
	}

	private void OnMarkersChanged(ARMarkersChangedEventArgs args)
	{
		foreach (var addedMarker in args.added)
		{
			HandleAddedMarker(addedMarker);
		}

		foreach (var updatedMarker in args.updated)
		{
			HandleUpdatedMarker(updatedMarker);
		}

		foreach (var removedMarkerId in args.removed)
		{
			HandleRemovedMarker(removedMarkerId);
		}
	}

	private void HandleAddedMarker(ARMarker addedMarker)
	{
		QRCodePosition = addedMarker.transform.position;
		Debug.Log($"QR Code Detected! Marker ID: {addedMarker.trackableId} Position: {QRCodePosition}");

		// Check cooldown before calling GetAllNotesWrapper
		if (Time.time - lastUpdateTime >= cooldownDuration)
		{
			noteSystem.QRCodePosition = QRCodePosition;
			noteSystem.GetAllNotesWrapper();
			lastUpdateTime = Time.time;
		}
	}

	private void HandleUpdatedMarker(ARMarker updatedMarker)
	{
		QRCodePosition = updatedMarker.transform.position;
		Debug.Log($"QR Code updated! Marker ID: {updatedMarker.trackableId} Position: {QRCodePosition}");

		// Check cooldown before calling GetAllNotesWrapper
		if (Time.time - lastUpdateTime >= cooldownDuration)
		{
			noteSystem.QRCodePosition = QRCodePosition;
			noteSystem.GetAllNotesWrapper();
			lastUpdateTime = Time.time;
		}
	}

	private void HandleRemovedMarker(ARMarker removedMarkerId)
	{
		Debug.Log($"QR Code Removed! Marker ID: {removedMarkerId}");
	}
}

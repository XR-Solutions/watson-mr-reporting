using Microsoft.MixedReality.OpenXR;
using UnityEngine;

public class QRCodeBehaviour : MonoBehaviour
{
	[SerializeField] private ARMarkerManager markerManager;

	private void Start()
	{
		if (markerManager == null)
		{
			Debug.LogError("ARMarkerManager is not assigned.");
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
		Debug.Log($"QR Code Detected! Marker ID: {addedMarker.trackableId}");
	}


	private void HandleUpdatedMarker(ARMarker updatedMarker)
	{
		Debug.Log($"QR Code updated! Marker ID: {updatedMarker}");
	}

	private void HandleRemovedMarker(ARMarker removedMarkerId)
	{
		Debug.Log($"QR Code Removed! Marker ID: {removedMarkerId}");
	}
}

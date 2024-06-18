using Assets.Scripts.Models;
using MixedReality.Toolkit.SpatialManipulation;
using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NoteSystem))]
[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(ObjectManipulator))]
[RequireComponent(typeof(NoteComponent))]
public class NoteBehaviour : MonoBehaviour
{
	private NoteComponent noteComponent;
	private NoteSystem noteSystem;
	private ObjectManipulator objectManipulator;
	private TextMeshProUGUI textMeshPro;
	private ParentPosition parentPosition;

	private void Awake()
	{
		objectManipulator = GetComponent<ObjectManipulator>();
		noteSystem = FindObjectOfType<NoteSystem>();
		noteComponent = GetComponent<NoteComponent>();
		textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
		parentPosition = GetComponent<ParentPosition>();

		if (noteSystem == null)
		{
			Debug.LogError("No NoteSystem found in the scene!");
		}

		if (objectManipulator == null)
		{
			Debug.LogError("No ObjectManipulator found");
		}

		if (noteComponent == null)
		{
			Debug.LogError("No NoteComponent found");
		}

		if (parentPosition == null)
		{
			Debug.LogError("No ParentPosition found");
		}

	}

	public void CreateNote()
	{        // Instantiate a new instance of this GameObject
		GameObject newNoteObject = Instantiate(gameObject);
		newNoteObject.SetActive(true); // Ensure the new object is active

		// Get the NoteBehaviour component from the new instance
		NoteBehaviour newNoteBehaviour = newNoteObject.GetComponent<NoteBehaviour>();

		noteSystem = FindObjectOfType<NoteSystem>();

		if (noteSystem == null)
		{
			Debug.LogError("No NoteSystem found in the scene!");
		}

		// Assign noteSystem to the new instance's NoteBehaviour
		newNoteBehaviour.noteSystem = noteSystem;

		// Ensure the newNoteObject has the NoteComponent
		NoteComponent newNoteComponent = newNoteObject.GetComponent<NoteComponent>();
		if (newNoteComponent == null)
		{
			Debug.LogError("New instance does not have a NoteComponent.");
			return;
		}

		// Create metadata and a new note
		ParentPosition newNoteParentPosition = newNoteObject.GetComponent<ParentPosition>();
		if (newNoteParentPosition == null)
		{
			Debug.LogError("New instance does not have a ParentPosition.");
			return;
		}

		var position = newNoteParentPosition.GetParentSpawnPosition();
		var rotation = newNoteParentPosition.GetParentRotation();

		float[] positionArray = new float[] { position.x, position.y, position.z };
		float[] rotationArray = new float[] { rotation.x, rotation.y, rotation.z, rotation.w };
		float[] scaleArray = new float[] { newNoteObject.transform.lossyScale.x, newNoteObject.transform.lossyScale.y, newNoteObject.transform.lossyScale.z };

		var metadata = new ObjectMetadata(positionArray, rotationArray, scaleArray, newNoteObject.activeSelf);
		var id = Guid.NewGuid().ToString();
		var note = new Note(id, "New Note", "This is the description", TraceTypes.None, metadata);
		newNoteComponent.SetNoteData(note);

		// Update the text of the new instance's TextMeshPro
		newNoteBehaviour.UpdateText("New Note");

		// Start the coroutine on the new instance
		newNoteBehaviour.StartCoroutine(newNoteBehaviour.noteSystem.CreateNoteCoroutine(note, newNoteObject));
	}

	public void TouchEnded()
	{
		var note = noteComponent.GetNoteData();

		Vector3 position = this.transform.position;
		Quaternion rotation = this.transform.rotation;
		Vector3 scale = this.transform.localScale;

		float[] positionArray = new float[] { position.x, position.y, position.z };
		float[] rotationArray = new float[] { rotation.x, rotation.y, rotation.z, rotation.w };
		float[] scaleArray = new float[] { scale.x, scale.y, scale.z };

		var metadata = new ObjectMetadata(positionArray, rotationArray, scaleArray, this.isActiveAndEnabled);
		var newNote = new Note(note.Guid, note.Name, note.Description, note.TraceType, metadata);

		noteComponent.SetNoteData(newNote);

		StartCoroutine(noteSystem.UpdateNoteCoroutine(newNote, this.gameObject));
	}

	public void UpdateText(string newText)
	{
		if (textMeshPro != null)
		{
			textMeshPro.text = newText;
		}
		else
		{
			Debug.LogError("TextMeshPro component not found.");
		}
	}
}

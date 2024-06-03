using Assets.Scripts.Models;
using UnityEngine;

[RequireComponent(typeof(NoteSystem))]
public class NoteBehaviour : MonoBehaviour
{
	[SerializeField]
	private NoteSystem noteSystem;

	private void Awake()
	{
		noteSystem = GetComponent<NoteSystem>();
		SyncNote();
	}

	private void SyncNote()
	{
		var note = new Note("New Note", "This is the description", TraceTypes.None);
		StartCoroutine(noteSystem.CreateNoteCoroutine(note));
	}
}

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
		var note = new Note("", "", TraceTypes.None);
		noteSystem.SyncNote(note);
	}
}

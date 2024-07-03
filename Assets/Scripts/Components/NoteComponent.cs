using Assets.Scripts.Models;
using UnityEngine;

public class NoteComponent : MonoBehaviour
{
	public string Guid { get; private set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public TraceTypes TraceType { get; set; }
	public ObjectMetadata ObjectMetadata { get; set; }

	public void SetNoteData(Note note)
	{
		Guid = note.Guid;
		Name = note.Name;
		Description = note.Description;
		TraceType = note.TraceType;
		ObjectMetadata = note.ObjectMetadata;
	}

	public Note GetNoteData()
	{
		return new Note(Guid, Name, Description, TraceType, ObjectMetadata);
	}
}

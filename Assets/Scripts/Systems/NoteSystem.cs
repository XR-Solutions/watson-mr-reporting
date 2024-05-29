using Assets.Scripts.Http;
using System.Collections.Generic;
using UnityEngine;

public class NoteSystem : MonoBehaviour
{
	private readonly List<Note> notes = new();
	private IHttpClient<Note> noteHttpClient;

	private void Awake()
	{
		noteHttpClient = new NoteHttpClient(new System.Net.Http.HttpClient());
	}

	public bool SyncNote(Note note)
	{
		var posted = noteHttpClient.Post(note);

		if (posted == null)
		{
			return false;
		}

		notes.Add(note);
		return true;
	}

}

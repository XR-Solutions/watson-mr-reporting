using Assets.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NoteSystem : MonoBehaviour
{
	private const string baseUrl = "https://localhost:58200/api/v1";
	[SerializeField]
	private GameObject PrefabToInstantiate;
	private List<Note> Notes = new List<Note>();
	private List<GameObject> InstantiatedNotes = new List<GameObject>();
	private static bool awakeHasRun = false;
	private static readonly object lockObject = new object();

	private void Awake()
	{
		lock (lockObject)
		{
			if (!awakeHasRun)
			{
				Debug.Log("Loading all notes");
				StartCoroutine(GetAllNotes());
				Debug.Log("All notes loaded");

				awakeHasRun = true;
			}
		}
	}

	public void GetAllNotesWrapper()
	{
		ClearInstantiatedNotes();

		StartCoroutine(GetAllNotes());
	}

	private void ClearInstantiatedNotes()
	{
		foreach (var instantiatedNote in InstantiatedNotes)
		{
			Destroy(instantiatedNote);
		}
		InstantiatedNotes.Clear();
	}

	public IEnumerator CreateNoteCoroutine(Note note, GameObject instantiatedObject)
	{
		string url = $"{baseUrl}/Note";
		string jsonData = JsonUtility.ToJson(note);

		UnityWebRequest request = new UnityWebRequest(url, "POST");

		Debug.Log($"Sending ${jsonData} to {url}");
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError(request.error);
		}
		else
		{
			Notes ??= new List<Note>();

			Debug.Log("Note created: " + request.downloadHandler.text);
			Notes.Add(note);

			InstantiateNote(instantiatedObject, note);
		}
	}

	public IEnumerator UpdateNoteCoroutine(Note note, GameObject noteGameObject)
	{
		string url = $"{baseUrl}/Note";
		string jsonData = JsonUtility.ToJson(note);

		UnityWebRequest request = new UnityWebRequest(url, "PUT");

		Debug.Log($"Sending ${jsonData} to {url}");
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError(request.error);
		}
		else
		{
			Debug.Log("Note updated successfully");

			InstantiateNote(noteGameObject, note);
		}
	}

	public IEnumerator GetAllNotesCoroutine()
	{
		string url = $"{baseUrl}/Note/all";
		UnityWebRequest request = UnityWebRequest.Get(url);

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError(request.error);
		}
		else
		{
			string jsonResponse = request.downloadHandler.text;
			Debug.Log($"Response: {jsonResponse}"); // Log the raw JSON response

			ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);
			List<Note> notes = ConvertToUnityNotes(response.data);
			Debug.Log($"Number of notes: {notes.Count}");

			Notes.Clear();
			foreach (var note in notes)
			{
				Debug.Log($"Name: {note.Name}, Description: {note.Description}, TraceType: {note.TraceType}, Position: x:{note.ObjectMetadata.Position[0]} y: {note.ObjectMetadata.Position[1]} z: {note.ObjectMetadata.Position[2]}");

				Notes.Add(note);
			}
		}
	}

	private IEnumerator GetAllNotes()
	{
		var request = GetAllNotesCoroutine();
		yield return StartCoroutine(request);

		foreach (var note in Notes)
		{
			GameObject instantiated = Instantiate(PrefabToInstantiate);
			InstantiatedNotes.Add(instantiated);
			var noteComponent = instantiated.GetComponent<NoteComponent>();

			noteComponent.SetNoteData(note);
			InstantiateNote(instantiated, note);
		}
	}

	public void InstantiateNote(GameObject gameObject, Note note)
	{
		Vector3 position = new Vector3(note.ObjectMetadata.Position[0], note.ObjectMetadata.Position[1], note.ObjectMetadata.Position[2]);
		Quaternion rotation = new Quaternion(note.ObjectMetadata.Rotation[0], note.ObjectMetadata.Rotation[1], note.ObjectMetadata.Rotation[2], note.ObjectMetadata.Rotation[3]);
		gameObject.transform.SetPositionAndRotation(position, rotation);

		var parent = gameObject.transform.parent;
		gameObject.transform.localScale = new Vector3(note.ObjectMetadata.Scale[0], note.ObjectMetadata.Scale[1], note.ObjectMetadata.Scale[2]);

		gameObject.SetActive(note.ObjectMetadata.Enabled);

		var noteBehaviour = gameObject.GetComponent<NoteBehaviour>();
		if (noteBehaviour != null)
		{
			noteBehaviour.UpdateText(note.Name);
		}
	}
	private List<Note> ConvertToUnityNotes(List<RawNote> rawNotes)
	{
		var notes = new List<Note>();
		foreach (var rawNote in rawNotes)
		{
			var note = new Note
			(
				rawNote.guid,
				rawNote.name,
				rawNote.description,
				(TraceTypes)rawNote.traceType,
				new ObjectMetadata
				(
					new float[] { rawNote.objectMetadata.position[0], rawNote.objectMetadata.position[1], rawNote.objectMetadata.position[2] },
					new float[] { rawNote.objectMetadata.rotation[0], rawNote.objectMetadata.rotation[1], rawNote.objectMetadata.rotation[2], rawNote.objectMetadata.rotation[3] },
					new float[] { rawNote.objectMetadata.scale[0], rawNote.objectMetadata.scale[1], rawNote.objectMetadata.scale[2] },
					rawNote.objectMetadata.enabled
				)
			);
			notes.Add(note);
		}
		return notes;
	}

	[System.Serializable]
	public class NoteList
	{
		public List<Note> notes;
	}

	[System.Serializable]
	public class ApiResponse
	{
		public bool succeeded;
		public string message;
		public string[] errors;
		public List<RawNote> data;
	}

	[System.Serializable]
	public class RawNote
	{
		public string guid;
		public string name;
		public string description;
		public int traceType;
		public RawObjectMetadata objectMetadata;
	}

	[System.Serializable]
	public class RawObjectMetadata
	{
		public float[] position;
		public float[] rotation;
		public float[] scale;
		public bool enabled;
	}
}

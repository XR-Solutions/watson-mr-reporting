using Assets.Scripts.Components;
using Assets.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	[SerializeField]
	public PdfPageDisplay pdfPageDisplay;
	private int pdfPageNumber = 1;

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
		Debug.Log("Clearing all Notes");
		ClearInstantiatedNotes();

		StartCoroutine(GetAllNotes());
	}

	public void DownloadPdfWrapper()
	{
		Debug.Log("Started pdf download");

		StartCoroutine(DownloadPdfCoroutine());
	}

	private void DestroyNotesWithComponent<T>() where T : Component
	{
		T[] notes = FindObjectsOfType<T>();

		foreach (T note in notes)
		{
			Destroy(note.gameObject);
		}
	}

	private void ClearInstantiatedNotes()
	{
		// Reverse iteration to safely remove items
		for (int i = InstantiatedNotes.Count - 1; i >= 0; i--)
		{
			var instantiatedNote = InstantiatedNotes[i];
			Destroy(instantiatedNote);
			InstantiatedNotes.RemoveAt(i);
		}

		if (InstantiatedNotes.Any())
		{
			Debug.Log("Not all instantiate notes are deleted");
		}

		DestroyNotesWithComponent<NoteComponent>();

		Notes.Clear();
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

	public void GetFirstPdfPage()
	{
		this.pdfPageNumber = 1;
		StartCoroutine(GetPdfPageCoroutine(this.pdfPageNumber));
	}
	public void GetNextPdfPage()
	{
		this.pdfPageNumber++;
		StartCoroutine(GetPdfPageCoroutine(this.pdfPageNumber));
	}

	public void GetPreviousPdfPage()
	{
		this.pdfPageNumber--;
		StartCoroutine(GetPdfPageCoroutine(this.pdfPageNumber));
	}


	public void ReloadPdfPage()
	{
		StartCoroutine(GetPdfPageCoroutine(this.pdfPageNumber));
	}

	private IEnumerator GetPdfPageCoroutine(int pageNumber)
	{
		string url = $"{baseUrl}/Pdf/sporenmatrix/page/{pageNumber}";
		UnityWebRequest request = UnityWebRequest.Get(url);
		request.downloadHandler = new DownloadHandlerTexture();

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError(request.error);
		}
		else
		{
			Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
			pdfPageDisplay.SetPdfPageImage(texture);
		}
	}


	private IEnumerator GetAllNotesCoroutine()
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

			ApiResponse<List<RawNote>> response = JsonUtility.FromJson<ApiResponse<List<RawNote>>>(jsonResponse);
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

	public IEnumerator GetNoteByIdCoroutine(string noteId)
	{
		string url = $"{baseUrl}/Note/{noteId}"; // Modify URL based on your API endpoint
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

			ApiResponse<RawNote> response = JsonUtility.FromJson<ApiResponse<RawNote>>(jsonResponse);

			if (response != null && response.data != null)
			{
				var note = ConvertToNote(response.data);
				Debug.Log($"Name: {note.Name}, Description: {note.Description}, TraceType: {note.TraceType}, Position: x:{note.ObjectMetadata.Position[0]} y: {note.ObjectMetadata.Position[1]} z: {note.ObjectMetadata.Position[2]}");

				UpdateNoteIfExists(note);
			}
			else
			{
				Debug.LogError("Note not found");
			}
		}
	}

	private IEnumerator DownloadPdfCoroutine()
	{
		string url = $"{baseUrl}/Pdf/sporenmatrix";
		UnityWebRequest request = UnityWebRequest.Get(url);
		request.downloadHandler = new DownloadHandlerBuffer();

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError(request.error);
		}
		else
		{
			byte[] pdfData = request.downloadHandler.data;
			string filePath = Path.Combine(Application.persistentDataPath, "sporenmatrix.pdf");
			try
			{
				File.WriteAllBytes(filePath, pdfData);
				Debug.Log($"PDF downloaded and saved to {filePath}");
			}
			catch (IOException e)
			{
				Debug.LogError($"Failed to save PDF: {e.Message}");
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

	private void InstantiateNote(GameObject gameObject, Note note)
	{
		Vector3 position = new Vector3(note.ObjectMetadata.Position[0], note.ObjectMetadata.Position[1], note.ObjectMetadata.Position[2]);
		Quaternion rotation = new Quaternion(note.ObjectMetadata.Rotation[0], note.ObjectMetadata.Rotation[1], note.ObjectMetadata.Rotation[2], note.ObjectMetadata.Rotation[3]);
		gameObject.transform.SetPositionAndRotation(position, rotation);

		gameObject.transform.localScale = new Vector3(note.ObjectMetadata.Scale[0], note.ObjectMetadata.Scale[1], note.ObjectMetadata.Scale[2]);
		gameObject.SetActive(note.ObjectMetadata.Enabled);
		var noteBehaviour = gameObject.GetComponent<NoteBehaviour>();
		if (noteBehaviour != null)
		{
			Debug.Log($"Setting name to {note.Name}");
			noteBehaviour.UpdateText(note.Name);
		}

		// Get the renderer component of the GameObject (assuming it's a MeshRenderer)
		var renderer = gameObject.GetComponent<Renderer>();
		if (renderer != null && renderer.material != null)
		{
			// Set color based on TraceType
			if (traceTypeColorMap.TryGetValue(note.TraceType, out Color traceTypeColor))
			{
				renderer.material.color = traceTypeColor;
			}
			else
			{
				Debug.LogWarning($"TraceType '{note.TraceType}' does not have a color mapping.");
			}
		}
		else
		{
			Debug.LogWarning("Renderer or material not found on instantiated object.");
		}
	}
	private List<Note> ConvertToUnityNotes(List<RawNote> rawNotes)
	{
		var notes = new List<Note>();
		foreach (var rawNote in rawNotes)
		{
			var note = ConvertToNote(rawNote);
			notes.Add(note);
		}
		return notes;
	}

	private void UpdateNoteIfExists(Note newNote)
	{
		bool noteFound = false;

		Debug.LogWarning($"Searching for note with Id '{newNote.Guid}' in list of size {Notes.Count}");
		for (int i = 0; i < Notes.Count; i++)
		{
			if (Notes[i].Guid == newNote.Guid)
			{
				Notes[i] = newNote;
				UpdateInstantiatedNote(InstantiatedNotes[i], newNote);
				noteFound = true;
				break;
			}
		}

		if (!noteFound)
		{
			Debug.LogWarning($"Note with ID {newNote.Guid} does not exist and will not be added.");
		}
	}

	private void UpdateInstantiatedNote(GameObject instantiatedNote, Note note)
	{
		Debug.LogWarning($"Found note with Id '{note.Guid}', updating it");
		Vector3 position = new Vector3(note.ObjectMetadata.Position[0], note.ObjectMetadata.Position[1], note.ObjectMetadata.Position[2]);
		Quaternion rotation = new Quaternion(note.ObjectMetadata.Rotation[0], note.ObjectMetadata.Rotation[1], note.ObjectMetadata.Rotation[2], note.ObjectMetadata.Rotation[3]);
		instantiatedNote.transform.SetPositionAndRotation(position, rotation);

		instantiatedNote.transform.localScale = new Vector3(note.ObjectMetadata.Scale[0], note.ObjectMetadata.Scale[1], note.ObjectMetadata.Scale[2]);

		instantiatedNote.SetActive(note.ObjectMetadata.Enabled);

		var noteBehaviour = instantiatedNote.GetComponent<NoteBehaviour>();
		if (noteBehaviour != null)
		{
			noteBehaviour.UpdateText(note.Name);
		}

		var noteComponent = instantiatedNote.GetComponent<NoteComponent>();
		if (noteComponent != null)
		{
			noteComponent.Description = note.Description;
		}

		// Get the renderer component of the GameObject (assuming it's a MeshRenderer)
		var renderer = gameObject.GetComponent<Renderer>();
		if (renderer != null && renderer.material != null)
		{
			// Set color based on TraceType
			if (traceTypeColorMap.TryGetValue(note.TraceType, out Color traceTypeColor))
			{
				renderer.material.color = traceTypeColor;
			}
			else
			{
				Debug.LogWarning($"TraceType '{note.TraceType}' does not have a color mapping.");
			}
		}
		else
		{
			Debug.LogWarning("Renderer or material not found on instantiated object.");
		}

		Debug.Log($"Name: {note.Name}, Description: {note.Description}, TraceType: {note.TraceType}, Position: x:{note.ObjectMetadata.Position[0]} y: {note.ObjectMetadata.Position[1]} z: {note.ObjectMetadata.Position[2]}");
	}

	private Note ConvertToNote(RawNote rawNote)
	{
		return new Note
		(
			rawNote.guid,
			rawNote.name,
			rawNote.description,
			(TraceTypes)rawNote.traceType,
			new Assets.Scripts.Models.ObjectMetadata
			(
				new float[] { rawNote.objectMetadata.position[0], rawNote.objectMetadata.position[1], rawNote.objectMetadata.position[2] },
				new float[] { rawNote.objectMetadata.rotation[0], rawNote.objectMetadata.rotation[1], rawNote.objectMetadata.rotation[2], rawNote.objectMetadata.rotation[3] },
				new float[] { rawNote.objectMetadata.scale[0], rawNote.objectMetadata.scale[1], rawNote.objectMetadata.scale[2] },
				rawNote.objectMetadata.enabled
			)
		);
	}

	[System.Serializable]
	public class NoteList
	{
		public List<Note> notes;
	}

	[System.Serializable]
	public class ApiResponse<T>
	{
		public bool succeeded;
		public string message;
		public string[] errors;
		public T data;
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

	private static readonly Dictionary<TraceTypes, Color> traceTypeColorMap = new Dictionary<TraceTypes, Color>
	{
		{ TraceTypes.None, Color.gray },
		{ TraceTypes.Bloodlike, Color.red },
		{ TraceTypes.Blood, new Color(0.7f, 0, 0) }, // Dark red
        { TraceTypes.Pattern, Color.blue },
		{ TraceTypes.Hull, Color.green },
		{ TraceTypes.Bullet, Color.cyan },
		{ TraceTypes.Hair, new Color(0.78f, 0.6f, 0.4f) }, // Brown
        { TraceTypes.Evidence, Color.yellow },
		{ TraceTypes.Seed, new Color(0.8f, 0.5f, 0.2f) }, // Orange
        { TraceTypes.Paint, Color.magenta },
		{ TraceTypes.Drugs, new Color(0.5f, 0.2f, 0.8f) }, // Purple
        { TraceTypes.Epithelium, new Color(1, 0.6f, 0.8f) }, // Light pink
        { TraceTypes.GSR, new Color(0.1f, 0.8f, 0.1f) }, // Dark green
        { TraceTypes.Fingerprint, new Color(0.9f, 0.9f, 0) }, // Yellow
        { TraceTypes.Palmprint, new Color(1, 0.7f, 0.4f) }, // Light orange
        { TraceTypes.Urine, new Color(0.6f, 0.4f, 0.2f) }, // Brown
        { TraceTypes.Scratch, new Color(0.5f, 0.5f, 0.5f) } // Gray
    };
}

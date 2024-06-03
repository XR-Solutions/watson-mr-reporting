using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NoteSystem : MonoBehaviour
{
	private const string baseUrl = "https://localhost:58200"; // Replace with your server URL

	public IEnumerator CreateNoteCoroutine(Note note)
	{
		string url = $"{baseUrl}/note";

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
			Debug.Log("Note created: " + request.downloadHandler.text);
		}
	}

	IEnumerator GetAllNotesCoroutine()
	{
		string url = $"{baseUrl}/notes";
		UnityWebRequest request = UnityWebRequest.Get(url);

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError(request.error);
		}
		else
		{
			string jsonResponse = request.downloadHandler.text;
			List<Note> notes = JsonUtility.FromJson<NoteList>(jsonResponse).notes;
			Debug.Log($"Number of notes: {notes.Count}");
			foreach (var note in notes)
			{
				Debug.Log($"Name: {note.Name}, Description: {note.Description}, TraceType: {note.TraceType}");
			}
		}
	}

	[System.Serializable]
	public class NoteList
	{
		public List<Note> notes;
	}

}

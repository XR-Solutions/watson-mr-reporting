using Assets.Scripts.Http;
using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

public class NoteHttpClient : IHttpClient<Note>
{
	private readonly HttpClient httpClient;

	public NoteHttpClient(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public IEnumerable Post(Note Object)
	{
		UnityWebRequest request = UnityWebRequest.Post("", "", "");

		yield return request.SendWebRequest();

		Debug.Log("Status Code: " + request.responseCode);
	}
}

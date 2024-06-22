using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Client
{
	public class SignalRClient : MonoBehaviour
	{
		private HubConnection connection;
		[SerializeField]
		private NoteSystem noteSystem;

		async void Start()
		{
			if (noteSystem == null)
			{
				Debug.LogWarning("No NoteSystem found, trying to add it automatically.");
				noteSystem = FindObjectOfType<NoteSystem>();
			}

			connection = new HubConnectionBuilder()
				.WithUrl("http://192.168.178.61:80/noteshub")
				.WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
				.Build();

			connection.On<string>("ReceiveNoteUpdate", noteId =>
			{
				Debug.Log($"SignalR Note updated: {noteId}");
				StartCoroutine(noteSystem.GetNoteByIdCoroutine(noteId));
			});

			connection.Reconnecting += error =>
			{
				Debug.LogWarning($"Reconnecting due to: {error?.Message}");
				return Task.CompletedTask;
			};

			connection.Reconnected += connectionId =>
			{
				Debug.Log($"Reconnected with connection ID: {connectionId}");
				return Task.CompletedTask;
			};

			connection.Closed += async error =>
			{
				Debug.LogError($"Connection closed due to: {error?.Message}");
				await Task.Delay(TimeSpan.FromSeconds(10));
				await StartConnectionAsync();
			};

			await StartConnectionAsync();
		}

		private async Task StartConnectionAsync()
		{
			var attempts = 0;
			while (true)
			{
				Debug.LogError($"Starting connection attempt {attempts}");
				try
				{
					await connection.StartAsync();
					Debug.Log("Connection started");
					break;
				}
				catch (Exception ex)
				{
					if (attempts >= 3)
					{
						return;
					}
					Debug.LogError($"Failed to connect: {ex.Message}");
					attempts++;
					await Task.Delay(TimeSpan.FromSeconds(10));
				}
			}
		}

		async void OnApplicationQuit()
		{
			await connection.StopAsync();
			await connection.DisposeAsync();
		}
	}
}

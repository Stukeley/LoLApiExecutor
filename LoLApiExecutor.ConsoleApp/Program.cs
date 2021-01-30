using LCUSharp;
using LCUSharp.Websocket;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LoLApiExecutor.ConsoleApp
{
	internal class Program
	{
		// API instance
		private static LeagueClientApi _api;

		// Event Handlers
		public static event EventHandler<LeagueEvent> GameFlowChanged;

		public static async void Connect()
		{
			// Setup the console.

			Console.InputEncoding = Encoding.Unicode;
			Console.OutputEncoding = Encoding.Unicode;

			// Initialize connection.
			_api = await LeagueClientApi.ConnectAsync();
			await _api.RiotClientEndpoint.ShowUxAsync();
			Console.WriteLine("Connected!");

			// Subscribe to necessary events.
			_api.Disconnected += Api_Disconnected;

			GameFlowChanged += OnGameFlowChanged;
			_api.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowChanged);
		}

		private static void OnGameFlowChanged(object sender, LeagueEvent e)
		{
			var result = e.Data.ToString();
			var state = string.Empty;

			if (result == "None")
			{
				state = "main menu";
			}
			else if (result == "ChampSelect")
			{
				state = "champ select";
			}
			else if (result == "Lobby")
			{
				state = "lobby";
			}
			else if (result == "InProgress")
			{
				state = "game";
			}

			// Print new state and set work to complete.
			Console.WriteLine($"Status update: Entered {state}.");
		}

		private static async void DisplayMenu()
		{
			Console.WriteLine("\n1 - change your summoner icon");
			Console.WriteLine("\n2 - change your status message");

			var input = char.Parse(Console.ReadLine());

			switch (input)
			{
				case '1':
					{
						Console.WriteLine("\nInsert the chosen icon ID.");

						var id = int.Parse(Console.ReadLine());

						var body = new { profileIconId = id };
						var queryParameters = Enumerable.Empty<String>();
						var json = await _api.RequestHandler.GetJsonResponseAsync(HttpMethod.Post, "lol-summoner/v1/current-summoner/icon", queryParameters, body);

						Console.WriteLine($"Response:\n\n{json}");
					}
					break;

				case '2':
					{
						// Just get and display user info for now.
						var json = await _api.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "​lol-chat​/v1​/me");

						Console.WriteLine($"Response:\n\n{json}");
					}
					break;
			}
		}

		private static void Api_Disconnected(object sender, EventArgs e)
		{
			Console.WriteLine("Disconnected!");
		}

		public static void Main()
		{
			try
			{
				Connect();

				DisplayMenu();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			_api.Disconnect();
		}
	}
}

using RiotSharp;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Misc;
using System;
using System.Linq;
using System.Threading;

namespace LoLDuoWinrateChecker
{
	internal class Program
	{
		/// <summary>
		/// The Riot API instance
		/// </summary>
		private static RiotApi Api;

		/// <summary>
		/// API key from Riot - sensitive info!
		/// </summary>
		private static string ApiKey = "";

		/// <summary>
		/// Variable in charge of limiting rates
		/// </summary>
		private static int AmountOfRequests = 0;

		/// <summary>
		/// Summoner name of the player to check
		/// </summary>
		public static string Summoner1 = "Sylrael";

		/// <summary>
		/// Summoner name of Summoner1's duo
		/// </summary>
		public static string Summoner2 = "Sempre";

		/// <summary>
		/// Region to check
		/// </summary>
		public static Region SummonerRegion = Region.Eune;

		/// <summary>
		/// Amount of matches to take from match history - the actual size can be much lower than this
		/// </summary>
		public static int MatchesToTake = 298;

		static Program()
		{
			Api = RiotApi.GetDevelopmentInstance(ApiKey);
		}

		/// <summary>
		/// Retrieve match history for Summoner1
		/// </summary>
		/// <returns></returns>
		public static MatchList RetrieveMatchHistory()
		{
			MatchList matchHistory;

			try
			{
				var summoner = Api.Summoner.GetSummonerByNameAsync(SummonerRegion, Summoner1).Result;
				var accId = summoner.AccountId;//account ID
				matchHistory = Api.Match.GetMatchListAsync(SummonerRegion, accId).Result;
				AmountOfRequests += 2;
			}
			catch (Exception e)
			{
				Console.WriteLine($"Exception: {e.Message}");
				throw;
			}

			Console.WriteLine("Got match history of Summoner1 successfully");

			return matchHistory;
		}

		/// <summary>
		/// Get duo winrate of Summoner1 and Summoner2 as well as the amount of wins and losses together
		/// </summary>
		public static void GetDuoWinrate()
		{
			var matchHistory = RetrieveMatchHistory();

			int losses = 0;
			int wins = 0;

			Console.WriteLine($"Match history capacity: {matchHistory.Matches.Capacity}");

			foreach (var matchReference in matchHistory.Matches.Take(MatchesToTake))
			{
				var isSummonerPresent = false;
				int teamId = 0;//Team ID of the summoners
				Match match;

				try
				{
					match = Api.Match.GetMatchAsync(SummonerRegion, matchReference.GameId).Result;
					AmountOfRequests++;
				}
				catch (Exception e)
				{
					Console.WriteLine($"Exception: {e.Message}");

					var partialWinrate = Math.Round((double)wins / (wins + losses), 3);
					Console.WriteLine($"Duo winrate of {Summoner1} and {Summoner2} is: {partialWinrate * 100}% with {wins} wins and {losses} losses " +
						$"(a total of {wins + losses} games together)");

					throw;
				}

				var matchParticipantIdentities = match.ParticipantIdentities;

				foreach (var matchParticipantIdentity in matchParticipantIdentities)
				{
					if (matchParticipantIdentity.Player.SummonerName == Summoner2)
					{
						isSummonerPresent = true;
						var participantId = matchParticipantIdentity.ParticipantId;

						teamId = match.Participants.First(x => x.ParticipantId == participantId).TeamId;

						break;
					}
				}

				if (!isSummonerPresent)
				{
					continue;
				}
				else
				{
					var participants = match.Participants;

					foreach (var participant in participants)
					{
						if (participant.TeamId == teamId)
						{
							if (participant.Stats.Winner)
							{
								wins++;
							}
							else
							{
								losses++;
							}
							break;
						}
					}
				}

				//Rate limits

				if (AmountOfRequests == 100)
				{
					Thread.Sleep(120000);
					AmountOfRequests = 0;
				}
				else if (AmountOfRequests % 20 == 0)
				{
					Thread.Sleep(1000);
				}
			}

			var winrate = Math.Round((double)wins / (wins + losses), 3);

			Console.WriteLine($"Duo winrate of {Summoner1} and {Summoner2} is: {winrate * 100}% with {wins} wins and {losses} losses " +
				$"(a total of {wins + losses} games together)");
		}

		private static void Main()
		{
			GetDuoWinrate();
		}
	}
}

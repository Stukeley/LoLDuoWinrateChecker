# LoLDuoWinrateChecker
A small console application for finding the winrate of two players. Originally made to check my winrate with a premade of mine.
Built using the RiotSharp wrapper (https://github.com/BenFradet/RiotSharp)

# How It Works
The app retrieves the match history for a predefined player, then checks every match that contains both summoners (the player and their premade).
Works for all game modes.

# What Is Needed
A pre-defined summoner name, premade name and server. Also an API key from Riot Games (https://developer.riotgames.com). This is a console application based on .NET Core 2.2.
All changes have to be made right in the code, inside the Config region.

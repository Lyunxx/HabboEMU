using System;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Minigames.Mafia_Wars
{
    interface IGame
    {
        bool AddUserToGame(GameClient User, string Team);

        bool RemoveUserFromGame(GameClient User, string Team, bool Alert, bool disconnect);

        void StartGame(bool Forced);

        void EndGame();

        void TeamWon(string Team);
    }
}

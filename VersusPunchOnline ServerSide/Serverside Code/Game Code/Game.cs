using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace VersusPunchOnline {
    public class Player : BasePlayer {
        public string ipAddress;
        public string port;
    }

    [RoomType("Lobby")]
    public class GameCode : Game<Player> {
        #region Variables
        private AppConst _appConst = new AppConst();
        private Random _random = new Random();
        #endregion


        #region Player.IO Methods
        public override void GameStarted() {
            Console.WriteLine($"Room start : {RoomId}");
        }

        public override void GameClosed() {
            Console.WriteLine($"Room close : {RoomId}");
        }

        public override void UserJoined(Player player) {
            List<string> parameters = new List<string>();

            foreach (Player p in Players) {
                parameters.Add(p.Id.ToString());
                parameters.Add(p.ConnectUserId);
            }

            player.Send(_appConst.serverMessageJoin, parameters.ToArray());
        }

        public override void UserLeft(Player player) {

        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message m) {
            LogMessage(m);
            string[] infos;

            switch (m.Type) {
                case "usermessage_requestp2p":
                    GetPlayerFromID(m.GetString(0)).Send(_appConst.serverMessageRequest, $"{player.Id},{player.ConnectUserId}");
                    break;

                case "usermessage_acceptrequest":
                    Player requester = GetPlayerFromID(m.GetString(0));

                    infos = new string[] { requester.Id.ToString(), "0" };
                    player.Send(_appConst.serverMessageAskForConnectionInfos, infos);
                    infos = new string[] { player.Id.ToString(), "1" };
                    requester.Send(_appConst.serverMessageAskForConnectionInfos, infos);
                    break;

                case "usermessage_p2popen":
                    infos = new string[] { m.GetString(1), m.GetString(2) };
                    GetPlayerFromID(m.GetString(0)).Send(_appConst.serverMessageConnectionInfos, infos);
                    break;
            }
        }
        #endregion

        #region Custom Methods
        private void LogMessage(Message m) {
            StringBuilder b = new StringBuilder();
            b.Append($"{m.Type} > ");
            for (int i = 0; i < m.Count; i++) {
                b.Append(m[(uint)i]);
                if (i < m.Count - 1)
                    b.Append(" ; ");
            }

            Console.WriteLine(b.ToString());
        }

        private Player GetPlayerFromID(string id) {
            foreach (Player p in Players)
                if (p.Id == int.Parse(id))
                    return p;

            return null;
        }
        #endregion
    }
}
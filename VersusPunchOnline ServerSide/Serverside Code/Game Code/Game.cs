using System;
using System.Deployment.Internal;
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
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (Player p in Players) {
                sb.Append(p.Id);
                sb.Append(",");
                sb.Append(p.ConnectUserId);

                count++;
                if (count < PlayerCount)
                    sb.Append(";");
            }
            player.Send(_appConst.serverMessageJoin, sb.ToString());
        }

        public override void UserLeft(Player player) {

        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message m) {
            LogMessage(m);

            switch (m.Type) {
                case "usermessage_requestptp":
                    GetPlayerFromID(m.GetString(0)).Send(_appConst.serverMessageRequest, $"{player.Id},{player.ConnectUserId}");
                    break;

                case "usermessage_acceptrequest":
                    Player requester = GetPlayerFromID(m.GetString(0));

                    player.Send(_appConst.serverMessageAskForConnectionInfos, $"{requester.Id};{0}");
                    requester.Send(_appConst.serverMessageAskForConnectionInfos, $"{player.Id};{1}");
                    break;

                case "usermessage_ptpopen":
                    GetPlayerFromID(m.GetString(0)).Send(_appConst.serverMessageConnectionInfos, $"{m.GetString(1)};{m.GetString(2)}");
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
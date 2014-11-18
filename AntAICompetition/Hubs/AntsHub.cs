using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace AntAICompetition.Hubs
{
    public class AntsHub : Hub
    {
        /// <summary>
        /// Client asks to listen to game so add them to the Game{Id} group
        /// </summary>
        /// <param name="gameId"></param>
        public void Listen(int gameId)
        {
            Groups.Add(Context.ConnectionId, "game" + gameId);
        }
    }
}
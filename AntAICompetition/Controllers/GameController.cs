using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AntAICompetition.Models;

namespace AntAICompetition.Controllers
{
    /// <summary>
    /// Api specification for the Ant AI Simulation
    /// </summary>
    public class GameController : ApiController
    {
        /// <summary>
        /// Initiates an agent logon with the simulation server by name. Once an agent is logged on, 
        /// a logon result is returned with the id and starting time of the next game.
        /// </summary>
        /// <param name="agentName"></param>
        /// <returns>LogonResult</returns>
        [HttpPost]
        [Route("api/game/logon")]
        public LogonResult Logon(string agentName)
        {
            // todo implement logon
            return new LogonResult();
        }
        /// <summary>
        /// Returns the full status for a certain game
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/game/{id}/status")]
        public GameStatus Status(int id)
        {
            // todo implement status
            return new GameStatus();
        }

        /// <summary>
        /// Returns just the turn information for a certain game
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/game/{id}/turn")]
        public TurnStatus Turn(int id)
        {
            // todo implement turn
            return new TurnStatus();
        }

        /// <summary>
        /// Updates the state of an agent's ants. Can be called once per turn.
        /// </summary>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/game/update")]
        public UpdateResult Update(UpdateRequest updateRequest)
        {
            // todo implement update
            return new UpdateResult();
        }


    }
}

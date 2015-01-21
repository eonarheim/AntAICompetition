(function () { 
    'use strict';
    // Backing engine to help kickstart development
    var url = require('url');
    var Promise = require('./promise.js');
    var Client = require('node-rest-client').Client;
    
    
    var AgentEngine = function (main, name, endpoint) {
        this.name = name || 'Node Agent';
        this.endpoint = endpoint || 'http://antsgame.azurewebsites.net';
        
        if (!main) throw new Error('Must implement a mainloop callback for Agent Engine');
        this.main = main;
        
        this.running = false;
        this.gameId = -1;
        this.authToken = '';
        this._pendingMoveRequests = [];
        
        this.client = new Client();

    };
    
    /**
 * Logs the agent onto the server and returns a promise that indicates completion
 */
AgentEngine.prototype.logon = function (gameId) {
        
        var args = {
            data : {
                'GameId' : gameId,
                'AgentName' : this.name
            },
            headers: { 'Content-Type': 'application/json' }
        };
        
        var promise = new Promise();
        
        var agent = this;
        this.client.post(url.resolve(this.endpoint, 'api/game/logon'), args, function (respose) {
            var data = JSON.parse(respose);
            agent.gameId = data.GameId;
            agent.authToken = data.AuthToken;
            console.log('Your game Id is ' + data.GameId);
            promise.resolve();
        });
        return promise;
    };
    
    /**
 * Moves ants up, down, left, or right every update
 */
AgentEngine.prototype.moveAnt = function (ant, direction) {
        if (this._pendingMoveRequests.some(function (m) {
            return m.AntId === ant.Id;
        })) {
            console.warn('WARNING! A move request has already been issued for ant', ant.Id);
            return false;
        }
        
        this._pendingMoveRequests.push({
            'AntId': ant.Id,
            'Direction': direction
        });
        return true;
    };
    
    /**
 * Internal mainloop of the agent engine
 */
AgentEngine.prototype.mainloop = function () {
        if (!this.running) return;
        
        // retrieve current gamestate
        var gotGameState = new Promise();
        this.client.post(url.resolve(this.endpoint, 'api/game/' + this.gameId + '/status/' + this.authToken), {}, function (data) {
            gotGameState.resolve(JSON.parse(data));
        });
        
        
        // Call user code with current gamestate
        var agent = this;
        gotGameState.then(function (gameState) {
            agent.main.apply(agent, [agent, gameState]);
            
            // Post updates
            var args = {
                data: {
                    'AuthToken': agent.authToken,
                    'GameId': agent.gameId,
                    'MoveAntRequests': agent._pendingMoveRequests
                },
                headers: { 'Content-Type': 'application/json' }
            };
            var updatesComplete = new Promise();
            agent.client.post(url.resolve(agent.endpoint, 'api/game/update'), args, function () {
                agent._pendingMoveRequests.length = 0; //fastest way to clear an array
                updatesComplete.resolve();
            });
            
            // When finished sending updates to server reschedule callback
            updatesComplete.then(function () {
                setTimeout(agent.mainloop.bind(agent), gameState.MillisecondsUntilNextTurn);
            });
        });
    };
    
    /**
 * Start the agent
 */
AgentEngine.prototype.start = function (gameId) {
        this.running = true;
        var agent = this;
        this.logon(gameId).then(function () {
            agent.mainloop.call(agent);
        });

    };
    /**
 * Stop the agent
 */
AgentEngine.prototype.stop = function () {
        this.running = false;
    };
    
    module.exports = AgentEngine;



})();
var AgentEngine = require('./agentengine.js');

console.log('Starting Nodejs Demo agent');

var directions = ["up", "down", "left", "right"];

var getDirection = function(from, to) {
    var dirX = to.X - from.X;
    var dirY = to.Y - from.Y;
    // greatest diff in x
    var dir = "left";
    if (Math.abs(dirX) > Math.abs(dirY)) {
        dir = dirX > 0 ? "right" : "left";
    }
    else {
        dir = dirY > 0 ? "down" : "up";
    }
    return dir;
}

var getRandomDirection = function() {
    return directions[Math.floor(directions.length * Math.random())];
}

var getDistance = function(from, to) {
    return Math.sqrt(Math.pow(from.X - to.X, 2) + Math.pow(from.Y - to.Y, 2));
}

var directionOfClosestFood = function (ant, foodList) {
    var closest = null;
    var minDistance = 9999.0;
    for(var f in foodList){
        if (getDistance(ant, foodList[f]) < minDistance) {
            minDistance = getDistance(ant, foodList[f]);
            closest = foodList[f];
        }
    }
    if (closest != null) {
        return getDirection(ant, closest);
    }
    else {
        return getRandomDirection();
    }
    
};

var agentEngine = new AgentEngine(function (engine, gameState) {
    // todo implement your awesome agent here :)
    console.log("Current Turn", gameState.Turn, ": Time to next turn ", gameState.MillisecondsUntilNextTurn);
    gameState.FriendlyAnts.forEach(function (a) {
        engine.moveAnt(a, directionOfClosestFood(a, gameState.VisibleFood));
    });
    // Updates are sent to the server automagically at the end of mainloop!
},'Node Agent', 'http://antsgame.azurewebsites.net');


agentEngine.start();
import AgentBase

class SampleAgent(AgentBase.AgentBase):

    def __init__(self, name, endpoint):
        AgentBase.AgentBase.__init__(self, name, endpoint)

    def update(self, game_status):

        for ant in game_status.friendly_ants:
            self.move_ant(ant.id, 'up') #this is a terrible strategy by the way

agent = SampleAgent("Sample Python Agent", "http://antsgame.azurewebsites.net/")
agent.start()




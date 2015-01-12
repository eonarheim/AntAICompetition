import requests
import json
from collections import namedtuple

class AgentBase:

    def __init__(self, name, endpoint):
        self.__headers = {"Accept": "application/json", "Content-Type": "application/json"}
        self.__name = name
        self.__endpoint = endpoint
        self.__game_id = None
        self.__auth_token = None

    def logon(self, game_id=None):
        params = {'GameId': game_id, 'AgentName': self.__name}
        r = requests.post(self.__endpoint+'/api/game/logon', data=json.dumps(params), headers=self.__headers)
        self.__game_id = r.json()['GameId']
        self.__auth_token = r.json()['AuthToken']

    def get_turn_info(self):
        r = requests.get(self.__endpoint+'/api/game/{0}/turn'.format(self.__game_id), headers=self.__headers)
        return self._json_object_hook(r.json())

    def get_game_info(self):
        r = requests.post(self.__endpoint+'/api/game/{0}/status/{1}'.format(self.__game_id,self.__auth_token), headers=self.__headers)
        data = r.json()

        friendly_ants = map(self.__parse_ant, data['FriendlyAnts'])
        enemy_ants = map(self.__parse_ant, data['EnemyAnts'])
        enemy_hills = map(self.__parse_hill, data['EnemyHills'])
        visible_food = map(self.__parse_food, data['VisibleFood'])



        return GameStatus(data['IsGameOver'], data['Status'], data['GameId'], data['Turn'], data['TotalFood'],
                          self.__parse_hill(data['Hill']), data['FogOfWar'], data['MillisecondsUntilNextTurn'],
                          friendly_ants, enemy_ants, enemy_hills, visible_food, data['Walls'])

    def update_game_state(self, move_ant_requests):
        data = json.dumps({'AuthToken':self.__auth_token, 'GameId':self.__game_id, 'MoveAntRequests': move_ant_requests})
        r = requests.post(self.__endpoint+'/api/game/{0}/status/{1}'.format(self.__game_id,self.__auth_token), headers=self.__headers)


    def __json_object_hook(self, d):
        return namedtuple('X', d.keys())(*d.values())

    def __parse_hill(self, hill_json):
        return Hill(hill_json['X'],hill_json['Y'],hill_json['Owner'])

    def __parse_ant(self, ant_json):
        return Ant(ant_json['Id'], ant_json['X'], ant_json['Y'], ant_json['Owner'])

    def __parse_food(self, food_json):
        return Food(food_json['X'], food_json['Y'])

    def __parse_cell(self, cell_json):
        return Cell(cell_json['X'], cell_json['Y'], cell_json['Type'], self.__parse_ant(cell_json['Ant']))



#DTOs
class MoveAntRequest:
    def __init__(self, ant_id, direction):
        self.direction = direction
        self.ant_id = ant_id


class Hill:
    def __init__(self, x, y, owner):
        self.x = x
        self.y = y
        self.owner = owner


class Ant:
    def __init__(self, unique_id, x, y, owner):
        self.id = unique_id
        self.x = x
        self.y = y
        self.owner = owner

class Cell:
    def __init__(self, x, y, type, ant):
        self.x = x
        self.y = y
        self.type = type
        self.ant = ant


class Food:
    def __init__(self, x, y):
        self.x = x
        self.y = y


class GameStatus:
    def __init__(self, is_game_over, status, game_id, turn, total_food, hill, fog_of_war, milliseconds_until_next_turn,
                 friendly_ants, enemy_ants, enemy_hills, visible_food, walls):
        self.is_game_over = is_game_over
        self.status = status
        self.game_id = game_id
        self.turn = turn
        self.total_food = total_food
        self.hill = hill
        self.fog_of_war = fog_of_war
        self.milliseconds_until_next_turn = milliseconds_until_next_turn
        self.friendly_ants = friendly_ants
        self.enemy_ants = enemy_ants
        self.enemy_hills = enemy_hills
        self.visible_food = visible_food
        self.walls = walls



a = AgentBase('Python Test Agent', 'http://antsgame.azurewebsites.net')
a.logon()
print a.get_game_info()


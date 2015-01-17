import requests
import json
from time import sleep


class AgentBase:

    def __init__(self, name, endpoint):
        self.__headers = {"Accept": "application/json", "Content-Type": "application/json"}
        self.__endpoint = endpoint
        self.__pending_move_requests = []
        self.game_id = None
        self.time_to_next_turn = 0
        self.auth_token = None
        self.name = name

    def logon(self, game_id=None):
        params = {'GameId': game_id, 'AgentName': self.name}
        r = requests.post(self.__endpoint+'/api/game/logon', data=json.dumps(params), headers=self.__headers)
        self.game_id = r.json()['GameId']
        self.auth_token = r.json()['AuthToken']

    def get_turn_info(self):
        r = requests.get(self.__endpoint+'/api/game/{0}/turn'.format(self.game_id), headers=self.__headers)
        return self._json_object_hook(r.json())

    def get_game_info(self):
        r = requests.post(self.__endpoint+'/api/game/{0}/status/{1}'.format(self.game_id,self.auth_token), headers=self.__headers)
        data = r.json()

        friendly_ants = map(self.__parse_ant, data['FriendlyAnts'])
        enemy_ants = map(self.__parse_ant, data['EnemyAnts'])
        enemy_hills = map(self.__parse_hill, data['EnemyHills'])
        visible_food = map(self.__parse_food, data['VisibleFood'])
        self.time_to_next_turn = data['MillisecondsUntilNextTurn']

        return GameStatus(data['IsGameOver'], data['Status'], data['GameId'], data['Turn'], data['TotalFood'],
                          self.__parse_hill(data['Hill']), data['FogOfWar'], data['MillisecondsUntilNextTurn'],
                          friendly_ants, enemy_ants, enemy_hills, visible_food, data['Walls'], data['Width'], data['Height'])

    def move_ant(self, ant, direction):
        duplicate_request = reduce(lambda prev, nxt: prev and nxt.ant_id == ant.id, self.__pending_move_requests, False)
        if duplicate_request:
            print "WARNING! A move request has already been issued for ant {0}" .format(ant.id)
            return False

        self.__pending_move_requests.append(MoveAntRequest(ant, direction))
        return True

    def update(self, game_state):
        pass #UPDATE GAME LOGIC HERE

    def start(self):
        self.logon()
        is_running = False

        if not is_running:

            is_running = True
            while is_running:

                gs = self.get_game_info()
                if gs.is_game_over:
                    is_running = False
                    print("Game Over!")
                    print(gs.status)
                    break

                self.update(gs)
                self.send_update(self.__pending_move_requests)
                self.__pending_move_requests = []

                if self.time_to_next_turn > 0:
                    sleep(self.time_to_next_turn/1000)

    def send_update(self, move_ant_requests):
        data = json.dumps({'AuthToken': self.auth_token, 'GameId': self.game_id,
                           'MoveAntRequests': self.__serialize__ant_requests(move_ant_requests)})
        r = requests.post(self.__endpoint+'/api/game/update'.format(self.game_id, self.auth_token), data=data, headers=self.__headers)

    def __serialize__ant_requests(self, move_ant_requests):
        ret_data = []
        for move_ant_request in move_ant_requests:
            ret_data.append({'AntId': move_ant_request.ant_id, 'Direction': move_ant_request.direction})
        return ret_data


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
                 friendly_ants, enemy_ants, enemy_hills, visible_food, walls, width, height):
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
        self.width = width
        self.height = height

#agent = AgentBase("Python Agent", "http://antsgame.azurewebsites.net")
#agent.start()

import chess
import chess.engine
import chess.pgn
import datetime
import random

class Engine:
    name: str
    wins: int
    draws: int
    losses: int
    engine: chess.engine.SimpleEngine
    
    def __init__(self, name: str) -> None:
        self.name = name
        self.wins = 0
        self.draws = 0
        self.losses = 0
        self.engine = chess.engine.SimpleEngine.popen_uci(["../chess_cs/Chess-Challenge/bin/Debug/net6.0/Chess-Challenge", "uci", self.name])
    
    def __str__(self) -> str:
        return self.name
    
    def __repr__(self) -> str:
        return f"class: {self.__class__.__name__} name: {self.name}; wins: {self.wins}; draws: {self.draws}; losses: {self.losses}"


class Pair:
    white: Engine
    black: Engine
    white_to_move: bool = True
    
    def __init__(self, one: Engine, two: Engine) -> None:
        if random.randint(0, 1) == 0:
            self.white = one
            self.black = two
        else:
            self.white = two
            self.black = one
    
    def side(self):
        try:
            if self.white_to_move:
                return self.white
            else:
                return self.black
        finally:
            self.white_to_move = not self.white_to_move


engines_names = ["MyBot", "EvilBot"]
engines = [Engine(i) for i in engines_names]
rounds = 1

def get_random_fen():
    with open("Fens.txt") as f:
        fens = f.readlines()
        return random.choice(fens).strip()

def setup_game(pair: Pair, board: chess.Board):
    game = chess.pgn.Game.from_board(board)
    game.headers["Event"] = "Playing back and forth"
    game.headers["Date"] = str(datetime.datetime.now())
    game.headers["Site"] = "localhost"
    game.headers["White"] = pair.white.name
    game.headers["Black"] = pair.black.name
    game.headers["Round"] = str(round)
    game.headers["Result"] = "*"
    game.headers["FEN"] = board.fen()
    game.headers["SetUp"] = "1"
    return game

def finish_game(game: chess.pgn.Game, winner: Engine):
    # Check winner side
    if winner is None:
        game.headers["Result"] = "1/2 - 1/2"
    else:
        white_wins = True if winner.name == game.headers["White"] else False
        
        if white_wins:
            game.headers["Result"] = "1 - 0"
        else:
            game.headers["Result"] = "0 - 1"
        
    return game

def run_game(pair: Pair):
    global myBot_wins, evilBot_wins, draws

    board = chess.Board(get_random_fen())
    limit = chess.engine.Limit()
    game = setup_game(pair, board)
    node = game
    
    while not board.is_game_over():
        side = pair.side()
        res = side.engine.play(board, limit)
        if res.move is None:  # Just in case. Probably will never happen
            print("res.move is None")
            pair.side()
            break
        board.push(res.move)
        node = node.add_variation(res.move)
        print(f"[{datetime.datetime.now()}] {side.name} played {res.move}")

    node.end()
    
    if board.is_checkmate():  # Current side wins
        side.wins += 1
    else:
        pair.white.draws += 1
        pair.black.draws += 1
    
    game = finish_game(game, side)
    
    with open("game.pgn", "a") as f:
        f.write(str(game) + "\n\n")
    
    print("----------------------------------------------------------------")
    print(f"[{datetime.datetime.now()}] {pair.white.name}: {pair.white.wins}+ {pair.white.draws}= {pair.white.losses}-")
    print(f"[{datetime.datetime.now()}] {pair.black.name}: {pair.black.wins}+ {pair.black.draws}= {pair.black.losses}-")
    print("----------------------------------------------------------------\n\n")

while True:
    try:
        run_game(Pair(*engines))
    except KeyboardInterrupt:
        exit()

import chess
import chess.engine
import chess.pgn
import datetime
import random

myBot = chess.engine.SimpleEngine.popen_uci(["../chess_cs/Chess-Challenge/bin/Debug/net6.0/Chess-Challenge", "uci", "MyBot"])
evilBot = chess.engine.SimpleEngine.popen_uci(["../chess_cs/Chess-Challenge/bin/Debug/net6.0/Chess-Challenge", "uci", "EvilBot"])

myBot_wins = 0
evilBot_wins = 0
draws = 0

def total_games():
    return myBot_wins + evilBot_wins + draws

def setup_game(white: chess.engine.SimpleEngine):
    game = chess.pgn.Game()
    game.headers["Event"] = "Playing back and forth"
    game.headers["Date"] = str(datetime.datetime.now())
    game.headers["Site"] = "localhost"
    game.headers["White"] = "MyBot" if white == myBot else "EvilBot"
    game.headers["Black"] = "EvilBot" if white == myBot else "MyBot"
    game.headers["Round"] = total_games() + 1
    game.headers["Result"] = "*"
    # game.headers["FEN"] = fen
    # game.headers["SetUp"] = "1"
    return game

def finish_game(game: chess.pgn.Game, winner: chess.engine.SimpleEngine):
    # Check winner side
    if winner is None:
        game.headers["Result"] = "1/2 - 1/2"
    else:
        strWinner = "MyBot" if winner == myBot else "EvilBot"
        white_wins = True if strWinner == game.headers["White"] else False
        
        if white_wins:
            game.headers["Result"] = "1 - 0"
        else:
            game.headers["Result"] = "0 - 1"
        
    return game

def run_game():
    global myBot_wins, evilBot_wins, draws
    board = chess.Board()
    limit = chess.engine.Limit()
    side = myBot if total_games() % 2 == 1 else evilBot
    game = setup_game(side)
    node = game
    
    while not board.is_game_over():
        side = evilBot if side == myBot else myBot
        res = side.play(board, limit)
        board.push(res.move)
        node = node.add_variation(res.move)
        print(f"[{datetime.datetime.now()}] {"MyBot" if side == myBot else "EvilBot"} played {res.move}")

    node.end()
    
    if board.is_checkmate():  # Current side wins
        if side == myBot:
            myBot_wins += 1
        else:
            evilBot_wins += 1
    else:
        draws += 1
    
    game = finish_game(game, side)
    
    with open("game.pgn", "a") as f:
        f.write(str(game) + "\n\n")
    
    print("----------------------------------------------------------------")
    print(f"[{datetime.datetime.now()}] MyBot: {myBot_wins}+ {draws}= {evilBot_wins}-")
    print(f"[{datetime.datetime.now()}] EvilBot: {evilBot_wins}+ {draws}= {myBot_wins}-")
    print("----------------------------------------------------------------\n\n")

while True:
    try:
        run_game()
    except KeyboardInterrupt:
        exit()

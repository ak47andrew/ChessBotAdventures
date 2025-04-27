# TODO do something
import os
import sys
import sqlite3
import inspect

class commands:
    class database:
        @staticmethod
        def init(*args):
            if "--filename" in args:
                filename = args[args.index("--filename") + 1]
                if not filename.endswith(".db"):
                    print(f"[error] Invalid filename '{filename}'. Use a '.db' extension.")
                    return
            else:
                filename = "chess.db"
            if os.path.exists(filename) and "--force" not in args:
                print(f"[warning] Database '{filename}' already exists. Use --force to overwrite.")
                return
            db = sqlite3.connect(filename)
            db.execute("""
                CREATE TABLE IF NOT EXISTS engines (
                    id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    name    TEXT,
                    command TEXT,
                    elo     REAL,
                    K       REAL
                );
                """)
            db.execute("""
                CREATE TABLE IF NOT EXISTS games (
                    id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    event     TEXT,
                    date      TEXT,
                    round     INTEGER,
                    bot1_id   INTEGER REFERENCES engines (id) ON DELETE NO ACTION
                                                                ON UPDATE CASCADE,
                    bot2_id   INTEGER REFERENCES engines (id) ON DELETE NO ACTION
                                                                ON UPDATE CASCADE,
                    result    TEXT,
                    FEN_start TEXT,
                    FEN_end   TEXT,
                    moves     TEXT
                );
                """)
            print("[succsess] Created database")
    


def run(args: list[str]):
    init_args = " ".join(args)
    curr = commands
    try:
        while len(args) > 0:
            curr = getattr(curr, args.pop(0).lower())
            if inspect.isfunction(curr):
                curr(*args)
                return
    except AttributeError:
        print(f"Unknown command: {init_args}")
        sys.exit(1)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python tournament.py <category> [...] <command>")
        sys.exit(1)
    run(sys.argv[1:])  # Remove the script name from the command line arguments

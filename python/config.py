# TODO do something
import sys
import sqlite3

if sys.argv[1] == "database" and sys.argv[2] == "init":
    db = sqlite3.connect("chess.db")
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
    print("Created database. Now fill it up please")
else:
    print("NotYetImplemented :3")

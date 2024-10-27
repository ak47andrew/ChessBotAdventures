
# Chess bot adventures

This is a project for creating chess bot. Nothing too crazy or really intresting - just my pet project for having fun (that I'm not). If you want to help: please create a pull request, issue or discussion <3

## Run Locally

1. Clone the project

```bash
git clone https://github.com/ak47andrew/ChessBotAdventures.git
```

2. Install the python dependencies

```bash
cd ChessBotAdventures/python
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

3. Generate database schema
```bash
python tournament.py
```

4. Fill up the engine info
```bash
sqlite3 chess.db
```
```sql
-- If you don't know elo
INSERT INTO engines (name, command) VALUES (<name>, <command>);
-- If you know elo
INSERT INTO engines (name, command, elo) VALUES (<name>, <command>, <elo>);

-- Name - name of the bot. Used in logs and pgn. Type: TEXT
-- Command - The program or script where the program will send requests in UCI format. Type: TEXT
-- For bots that's written using Chess-Challenge API ("core" folder) specify chess_cs/Chess-Challenge/bin/Debug/net6.0/Chess-Challenge uci <class' name of the bot>
-- elo - elo of the bot. Type: REAL
```

5. To run a tournament run tournament.py once again
```bash
python tournament.py
```

6. To calculate elo of bot play
```bash
python get_elo.py <name>
# name is the bot's name in the database that you set in the step 4
```

## License

[MIT](https://choosealicense.com/licenses/mit/)


## Authors

- [@ak47andrew](https://www.github.com/ak47andrew)
- [@SebLague](https://github.com/SebLague)

## Acknowledgements

- [Chess-Challange API by SebLeague](https://github.com/SebLague/Chess-Challenge)
- [Raylib (C#, GUI)](https://www.raylib.com/)
- [Chess (Python, package)](https://pypi.org/project/chess/)
## Lessons Learned

- None
- Having fun in programming is enjoing 24/7 pain

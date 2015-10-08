using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace JustSnake
{
    struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    class SnakeGame
    {
        const int GameWidth = 70;
        const int GameHeight = 25;

        static int level = 1;
        static double sleepTime = 100;
        static int userScore = 0;
        static List<Tuple<string, int>> players = new List<Tuple<string, int>>();
        static string nickname = String.Empty;
        public static void Main()
        {
            ReadHighScore();
            StartMenu();
            Console.Clear();

            PrintBorder();

            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            int lastFoodTime = 0;
            int foodDissapearTime = 8000;


            Console.WindowHeight = GameHeight + 1;
            Console.BufferHeight = GameHeight + 1;
            Console.WindowWidth = GameWidth + 20;
            Console.BufferWidth = GameWidth + 20;

            Position[] directions = new Position[]
            {
                new Position(0, 1), // right
                new Position(0, -1), // left
                new Position(1, 0), // down
                new Position(-1, 0), // up
            };

            int direction = right;
            Random randomNumbersGenerator = new Random();

            lastFoodTime = Environment.TickCount;

            List<Position> obstacles =
                new List<Position>();

            Queue<Position> snakeElements =
                new Queue<Position>();

            for (int i = 1; i <= 5; i++)
            {
                snakeElements.Enqueue(new Position(1, i));
            }

            Position food;
            food = PrintFood(snakeElements, obstacles, randomNumbersGenerator);


            foreach (Position position in snakeElements)
            {
                Console.SetCursorPosition(
                    position.col,
                    position.row);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("*");
            }

            GameRunning(direction,right,left,up,down,obstacles,snakeElements,lastFoodTime,foodDissapearTime,randomNumbersGenerator,food,directions,nickname);
            
        }

        static void GameRunning(int direction, byte right, byte left, byte up, byte down
            , List<Position> obstacles, Queue<Position> snakeElements, int lastFoodTime, int foodDissapearTime
            ,Random randomNumbersGenerator,Position food,Position[] directions,string nickname)
        {
            while (true)
            {
                direction = keyAvailable(direction, right, left, up, down);

                Position snakeHead = snakeElements.Last();
                Position nextDirection = directions[direction];

                Position snakeNewHead = new Position(
                    snakeHead.row + nextDirection.row,
                    snakeHead.col + nextDirection.col);

                snakeNewHead = GameField(snakeNewHead);

                EndGame(snakeElements, obstacles, nickname, snakeNewHead);

                PrintScoreAndLevel(userScore, level);

                Console.SetCursorPosition(
                    snakeHead.col,
                    snakeHead.row);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("*");

                //snake head in any directions
                snakeElements.Enqueue(snakeNewHead);
                Console.SetCursorPosition(
                    snakeNewHead.col,
                    snakeNewHead.row);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (direction == right) Console.Write(">");
                if (direction == left) Console.Write("<");
                if (direction == up) Console.Write("^");
                if (direction == down) Console.Write("v");


                if (snakeNewHead.col == food.col &&
                    snakeNewHead.row == food.row)
                {
                    // feeding the snake
                    do
                    {
                        food = new Position(
                            randomNumbersGenerator.Next(1,
                                GameHeight - 3),
                            randomNumbersGenerator.Next(1,
                                GameWidth - 3));
                    }
                    while (snakeElements.Contains(food) ||
                                obstacles.Contains(food));

                    lastFoodTime = Environment.TickCount;
                    Console.SetCursorPosition(
                        food.col,
                        food.row);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("@");

                    sleepTime--;

                    //update score
                    userScore += 20;
                    PlayerLevel(userScore, randomNumbersGenerator, snakeElements, obstacles, food);
                }
                else
                {
                    // moving...
                    Position last = snakeElements.Dequeue();
                    Console.SetCursorPosition(last.col, last.row);
                    Console.Write(" ");
                }


                if (Environment.TickCount - lastFoodTime >=
                    foodDissapearTime)
                {

                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");
                    do
                    {
                        food = new Position(
                            randomNumbersGenerator.Next(1,
                            GameHeight - 3),
                            randomNumbersGenerator.Next(1,
                            GameWidth - 3));
                    }
                    while (snakeElements.Contains(food) ||
                        obstacles.Contains(food));
                    lastFoodTime = Environment.TickCount;
                }
                Console.SetCursorPosition(food.col, food.row);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("@");
                sleepTime -= 0.05;

                Thread.Sleep((int)sleepTime);
            }
        }

        
        static void PrintControlsInfo()
        {
            Console.Clear();
            Print(0, 0, @"
Controls:
================
[^] move up
[v] move down
[<] move left
[>] move right
================
            
",ConsoleColor.Green);
            Print(8, 0, @"
Controls in last level
Drunk snake
================
[^] move up
[v] move down
[>] move left
<] move right
================
            
",ConsoleColor.Green);
            Console.ReadKey(true);
            Console.Clear();
            StartMenu();
        }

        static void PrintBorder()
        {
            for (int col = 0; col < GameWidth; col++)
            {
                Print(0, col, '*',ConsoleColor.Green);
                Print(24, col, '*', ConsoleColor.Green);
            }
            for (int row = 0; row < GameHeight; row++)
            {
                Print(row, 0, '*', ConsoleColor.Green);
                Print(row, 69, '*', ConsoleColor.Green);
            }
        }

        static void Print(int row, int col, object data,ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(col, row);
            Console.Write(data);
        }

        static void EndGame(Queue<Position> snakeElements, List<Position> obstacles, string nickname, Position snakeNewHead)
        {
            if (snakeElements.Contains(snakeNewHead)
                    || obstacles.Contains(snakeNewHead))
            {
                Console.Clear();
                Console.CursorVisible = false;
                Console.SetCursorPosition(GameWidth / 2 + 5, GameHeight / 2);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Game over!");
                Console.ReadLine();
                UpdateScore(userScore, nickname);
                Environment.Exit(0);
                
            }
        }

        static Position PrintFood(Queue<Position> snakeElements, List<Position> obstacles, Random randomNumbersGenerator)
        {
            Position food;
            do
            {
                food = new Position(
                    randomNumbersGenerator.Next(1,
                        GameHeight - 3),
                    randomNumbersGenerator.Next(1,
                       GameWidth - 3));
            }
            while (snakeElements.Contains(food) ||
                   obstacles.Contains(food));
            Console.SetCursorPosition(food.col, food.row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("@");

            return food;
        }

        static void PrintScoreAndLevel(int userScore, int level)
        {
            // score player
            Console.SetCursorPosition(GameWidth + 5, 1);
            Console.WriteLine("Score: {0}", userScore);

            // level
            Console.SetCursorPosition(GameWidth + 5, 3);
            Console.WriteLine("Level: {0}", level);

            // best score
            var playerSortedScore = players.OrderByDescending(a => a.Item2).Take(1).Select(player => player.Item2);
            var playerSortedName = players.OrderByDescending(a => a.Item2).Take(1).Select(player => player.Item1);

            int score = int.Parse(string.Join("",playerSortedScore));
            string name = string.Join("",playerSortedName);
            Console.SetCursorPosition(GameWidth + 4, 7);
            Console.Write("Score: ");
            Console.WriteLine(score);
            Console.SetCursorPosition(GameWidth + 4, 8);
            Console.Write("By: ");
            Console.WriteLine(name);

            // field of best score
            for (int col = GameWidth + 2; col < GameWidth + 17; col++)
            {
                Print(5, col, '*', ConsoleColor.Green);
                Print(10, col, '*', ConsoleColor.Green);

            }
            for (int row = 6; row < 10; row++)
            {
                Print(row, GameWidth + 2, '*', ConsoleColor.Green);
                Print(row, GameWidth + 16, '*', ConsoleColor.Green);
            }
        }

        static void AddObstacle(Random randomNumbersGenerator, Queue<Position> snakeElements, List<Position> obstacles, Position food, int rolls)
        {
            for (int i = 0; i < rolls; i++)
            {
                Position obstacle = new Position();
                do
                {
                    obstacle = new Position(
                        randomNumbersGenerator.Next(1,
                            GameHeight - 3),
                        randomNumbersGenerator.Next(1,
                            GameWidth - 3));
                }
                while (snakeElements.Contains(obstacle) ||
                    (food.row != obstacle.row &&
                    food.col != obstacle.row));
                obstacles.Add(obstacle);
                Console.SetCursorPosition(
                    obstacle.col,
                    obstacle.row);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("=");

            }

        }

        static void ShowHighScores()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (players.Count == 0)
            {
                ReadHighScore();
            }
            var result = players.OrderByDescending(player => player.Item2)
                .Take(5)
                .Select(player => player.Item1 + " scored: " + player.Item2);

            Console.WriteLine("Top 5 scores:");
            Console.WriteLine(string.Join("\n", result));
            Console.WriteLine("Press any key to continue...");
            string key = Console.ReadLine();
            if (key != null)
            {
                Console.Clear();
                StartMenu();
            }

        }

        static void ReadHighScore()
        {
            string path = "../../highscores.txt";
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    string[] data = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = data[0];
                    int score = int.Parse(data[1]);

                    players.Add(new Tuple<string, int>(name, score));
                }
            }
        }

        static void UpdateScore(int Score, string nickname)
        {
            Console.CursorVisible = true;
            string path = "../../highscores.txt"; 
            Console.Clear();
            Console.WriteLine("\nWould you like to save your score?Yes/No");
            Console.Write("Please enter: ");
            string option = Console.ReadLine().ToLower();
            if (option == "Yes".ToLower())
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine(nickname + " " + userScore);
                }
            }
            Console.Clear();
            
        }

        static Position GameField(Position snakeNewHead)
        {
            if (snakeNewHead.col <= 0)
            {
                snakeNewHead.col = 1;
            }
            if (snakeNewHead.row <= 0)
            {
                snakeNewHead.row = 1;
            }
            if (snakeNewHead.row >= GameHeight - 2)
            {
                snakeNewHead.row = GameHeight - 2;
            }
            if (snakeNewHead.col >= GameWidth - 2)
            {
                snakeNewHead.col = GameWidth - 2;
            }
            return snakeNewHead;
        }

        static int keyAvailable(int direction, byte right, byte left, byte up, byte down)
        {
            if (Console.KeyAvailable)
            {
                if (userScore <= 220)
                {
                    ConsoleKeyInfo userInput = Console.ReadKey();
                    if (userInput.Key == ConsoleKey.LeftArrow)
                    {
                        if (direction != right) direction = left;
                    }
                    if (userInput.Key == ConsoleKey.RightArrow)
                    {
                        if (direction != left) direction = right;
                    }
                    if (userInput.Key == ConsoleKey.UpArrow)
                    {
                        if (direction != down) direction = up;
                    }
                    if (userInput.Key == ConsoleKey.DownArrow)
                    {
                        if (direction != up) direction = down;
                    }
                }
                else
                {
                    ConsoleKeyInfo userInput = Console.ReadKey();
                    if (userInput.Key == ConsoleKey.LeftArrow)
                    {
                        if (direction != left) direction = right;
                    }
                    if (userInput.Key == ConsoleKey.RightArrow)
                    {
                        if (direction != right) direction = left;
                    }
                    if (userInput.Key == ConsoleKey.UpArrow)
                    {
                        if (direction != up) direction = down;
                    }
                    if (userInput.Key == ConsoleKey.DownArrow)
                    {
                        if (direction != down) direction = up;
                    }
                }
            }

            return direction;
        }

        static void StartMenu()
        {
            
            Console.Title = "Snake";
            Print(0, 0,@"                                  
                          ___   _ __     __ _  | | __   ___ 
                         / __| | '_ \   / _` | | |/ /  / _ \
                         \__ \ | | | | | (_| | |   <  |  __/
                         |___/ |_| |_|  \__,_| |_|\_\  \___|
                                    ", ConsoleColor.Green);
           
            Print(8,40,"Menu:", ConsoleColor.White);
            Print(10,30,"[1].Play game", ConsoleColor.White);
            Print(11,30,"[2].View best score", ConsoleColor.White);
            Print(12,30,"[3].Controls", ConsoleColor.White);
            Print(13,30,"[4].Exit", ConsoleColor.White);
            Console.SetCursorPosition(30, 14);
            Console.Write("Please enter options: ");
            int option;
            if (int.TryParse(Console.ReadLine(), out option))
            {
                Console.WriteLine();
                switch (option)
                {
                    case 1:
                        Console.Clear();
                        Console.Write("Plaese enter nickname: ");
                        nickname = Console.ReadLine();
                       PrintBorder();
                        break;
                    case 2:
                        ShowHighScores();
                        break;
                    case 3:
                        PrintControlsInfo();
                        break;
                    case 4:
                        Environment.Exit(0);
                        break;
                }
            }

        }

        static void PlayerLevel(int userPoints, Random randomNumbersGenerator
            , Queue<Position> snakeElements, List<Position> obstacles, Position food)
        {
            int rolls = 0;
            if (userPoints == 40)
            {
                level++;
                rolls = 5;
                AddObstacle(randomNumbersGenerator, snakeElements, obstacles, food, rolls);

            }
            else if (userPoints == 80)
            {
                level++;
                rolls = 6;
                AddObstacle(randomNumbersGenerator, snakeElements, obstacles, food, rolls);


            }
            else if (userPoints == 120)
            {
                level++;
                rolls = 7;
                AddObstacle(randomNumbersGenerator, snakeElements, obstacles, food, rolls);

            }
            else if (userPoints == 160)
            {
                level++;
                rolls = 8;
                AddObstacle(randomNumbersGenerator, snakeElements, obstacles, food, rolls);
            }
            else if (userPoints == 200)
            {
                level++;
                rolls = 9;
                AddObstacle(randomNumbersGenerator, snakeElements, obstacles, food, rolls);
            }

            else if ((userPoints == 240) || (userPoints == 280) || (userPoints == 320) || (userPoints == 360) || (userPoints == 400))
            {
                level++;
                rolls = 3;
                AddObstacle(randomNumbersGenerator, snakeElements, obstacles, food, rolls);

            }

        }
    }
}
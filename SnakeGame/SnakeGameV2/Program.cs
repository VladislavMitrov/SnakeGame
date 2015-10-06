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
        static string txtHighScore = String.Empty;
        static string txtNickname = String.Empty;
        static double sleepTime = 100;
        static int userScore = 0;


        static void Main(string[] args)
        {
            StartMenu();

            string nickname = Console.ReadLine();
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

        static void PrintBorder()
        {
            for (int col = 0; col < GameWidth; col++)
            {
                Print(0, col, '*');
                Print(24, col, '*');
            }
            for (int row = 0; row < GameHeight; row++)
            {
                Print(row, 0, '*');
                Print(row, 69, '*');
            }
        }

        static void Print(int row, int col, object data)
        {
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
                UpdateScore(userScore, nickname);
                Console.ReadLine();
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
            UpdateHighScore();
            // TODO: best score
            Console.SetCursorPosition(GameWidth + 4, 7);
            Console.Write("Score: ");
            Console.WriteLine(txtHighScore);
            Console.SetCursorPosition(GameWidth + 4, 8);
            Console.Write("By: ");
            Console.WriteLine(txtNickname);

            // field of best score
            for (int col = GameWidth + 2; col < GameWidth + 17; col++)
            {
                Print(5, col, '*');
                Print(10, col, '*');

            }
            for (int row = 6; row < 10; row++)
            {
                Print(row, GameWidth + 2, '*');
                Print(row, GameWidth + 16, '*');
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

        static void UpdateHighScore()
        {
            string path = "../../highscores.txt"; 
            if (File.Exists(path))
            {
                string line;
                StreamReader file = null;
                file = new StreamReader(path);
                string[] highScores = new string[100];
                int i = 0;
                while ((line = file.ReadLine()) != null)
                {
                    highScores[i] = line;
                    i++;
                }
                file.Close();
                string pattern = @"\d+";
                int best = 0;
                int score = 0;
                string name = String.Empty;
                for (int k = 0; k < 10; k++)
                {
                    Match matchScore = Regex.Match(highScores[k], pattern);
                    Match matchName = Regex.Match(highScores[k], "[a-zA-z]+");
                    score = int.Parse(matchScore.ToString());
                    if (score > best)
                    {
                        best = score;
                        name = matchName.ToString();
                    }

                }
                txtHighScore = best.ToString();
                txtNickname = name;

            }
        }

        static void UpdateScore(int Score, string nickname)
        {
            string path = "../../highscores.txt"; 
            string FinalScore = Score + " - " + nickname + Environment.NewLine;
            if (File.Exists(path))
            {
                StreamWriter writerScore;
                writerScore = File.AppendText(path);
                writerScore.WriteLine(FinalScore);
                writerScore.Close();
            }
            else
            {
                StreamWriter writerScore;
                writerScore = File.CreateText(path);
                writerScore.Close();
                writerScore = File.AppendText(path);
                writerScore.WriteLine(FinalScore);
                writerScore.Close();
            }
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
            Console.SetCursorPosition(36, 8);
            Console.WriteLine("Menu:");
            Console.WriteLine(@"
                            ====================
                            [^] move up
                            [v] move down
                            [<] move left
                            [>] move right
            
");

            Console.SetCursorPosition(28, 16);
            Console.Write("Please enter nickname: ");
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
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections;
using Raylib_cs;
using System.Collections.Generic;

namespace Graphics
{
    public class Program
    {
        static int windowHeight = 800;
        static int windowLength = 1000;
        static Rectangle p1 = new Rectangle(windowLength / 4, (windowHeight / 4) * 3, 25, 40);
        static List<Obstacle> obstacles = new List<Obstacle>();
        static List<Obstacle> buttons = new List<Obstacle>();
        static List<string> levels = new List<string>();
        static string currentLevel;
        static bool touchingGround;
        static bool jumping;
        static bool drawing = false;
        static float startingX;
        static float startingY;
        static float x;
        static float xStart;
        static float yStart;
        static int yOffset = 0;
        static float xVelocity;
        static float yVelocity;
        static string gameState;
        static string lastGamemode;
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(@"saveFolder");
            foreach (string line in files)
            {
                string name = line;
                name = name.Remove(name.Length - 4).Substring(11);
                levels.Add(name);
            }
            gameState = "menu";
            MenuButtons();
            Raylib.SetTargetFPS(120);
            Raylib.InitWindow(windowLength, windowHeight, "Test");
            while (!Raylib.WindowShouldClose())
            {
                Raylib.SetExitKey(0);
                if (gameState == "game" || gameState == "create")
                {
                    CheckKeyPresses();
                    UpdatePos();
                    if (gameState == "game") CheckCollision();
                }
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);
                if (gameState == "game" || gameState == "create") RenderScene();
                if (gameState == "pauseScreen" || gameState == "menu" || gameState == "levelSelect" || gameState == "gamemodeChoose") Menu();
                Raylib.EndDrawing();
            }
        }
        static void Menu()
        {
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP) && gameState == "levelSelect") yOffset -= 2;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN) && gameState == "levelSelect") yOffset += 2;
            drawing = false;
            for (int i = 0; i < windowHeight / 29; i++)
            {
                Raylib.DrawRectangle(0, i * 30, windowLength, 15, Color.LIGHTGRAY);
            }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) && gameState == "pauseScreen") gameState = "game";
            for (int i = 0; i < buttons.Count; i++)
            {
                Obstacle button = buttons[i];
                Raylib.DrawRectangle((int)button.x, (int)button.y + yOffset, (int)button.width, (int)button.height, Color.DARKGRAY);
                Raylib.DrawRectangle((int)button.x + 10, (int)button.y + 10 + yOffset, (int)button.width - 20, (int)button.height - 20, Color.GRAY);
                Raylib.DrawText(button.type.ToUpper(), (int)(button.x + button.width / 5), (int)(button.y + button.height / 4) + yOffset, 80, Color.DARKGRAY);
                if (Raylib.GetMouseX() > button.x && Raylib.GetMouseX() < button.x + button.width && Raylib.GetMouseY() > button.y + yOffset && Raylib.GetMouseY() < button.y + button.height + yOffset)
                {
                    Raylib.DrawRectangle((int)button.x + 10, (int)button.y + 10 + yOffset, (int)button.width - 20, (int)button.height - 20, Color.LIGHTGRAY);
                    Raylib.DrawText(button.type.ToUpper(), (int)(button.x + button.width / 5), (int)(button.y + button.height / 4) + yOffset, 80, Color.DARKGRAY);
                    if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                    {
                        foreach (string line in levels)
                        {
                            Console.WriteLine(line);
                        }
                        if (button.type == "newLevel")
                        {
                            obstacles.Clear();
                            RestartGameMode();
                            gameState = "create";
                        }
                        else if (button.type == "levelSelect" && levels.Any())
                        {
                            gameState = "levelSelect";
                            MenuButtons();
                        }
                        else if (button.type == "exit") Environment.Exit(0);
                        else if (button.type == "resume") gameState = lastGamemode;
                        else if (button.type == "save") SaveStage();
                        else if (button.type == "restart")
                        {
                            RestartGameMode();
                            x = xStart;
                            p1.y = yStart;
                            gameState = "game";
                        }
                        else if (button.type == "menu")
                        {
                            gameState = "menu";
                            MenuButtons();
                        }
                        else if (levels.Contains(button.type))
                        {
                            currentLevel = button.type;
                            gameState = "gamemodeChoose";
                            MenuButtons();
                        }
                        else if (button.type == "game")
                        {
                            gameState = "game";
                            RestartGameMode();
                            LoadStage();
                            x = xStart;
                            p1.y = yStart;
                        }
                        else if (button.type == "create")
                        {
                            gameState = "create";
                            RestartGameMode();
                            LoadStage();
                            x = xStart;
                            p1.y = yStart;
                        }
                    }
                }
            }
        }
        static void MenuButtons()
        {
            buttons.Clear();
            if (gameState == "menu")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 130, 500, 140, "newLevel"));
                buttons.Add(new Obstacle(windowLength / 2 - 250, 350, 500, 140, "levelSelect"));
                // buttons.Add(new Obstacle(windowLength / 2 - 250, 570, 500, 140, "exit"));
            }
            else if (gameState == "pauseScreen")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 130, 500, 140, "resume"));
                buttons.Add(new Obstacle(windowLength / 2 - 250, 570, 500, 140, "menu"));
            }
            else if (gameState == "levelSelect")
            {
                string[] files = Directory.GetFiles(@"saveFolder");
                for (int i = 0; i < files.Length; i++)
                {
                    string buttonName = files[i];
                    buttonName = buttonName.Remove(buttonName.Length - 4).Substring(11);
                    buttons.Add(new Obstacle(windowLength / 2 - 250, 50 + i * 180, 500, 125, buttonName));
                }
            }
            else if (gameState == "gamemodeChoose")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 130, 500, 140, "game"));
                buttons.Add(new Obstacle(windowLength / 2 - 250, 350, 500, 140, "create"));
            }
            if (gameState == "pauseScreen" && lastGamemode == "create")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 350, 500, 140, "save"));
            }
            else if (gameState == "pauseScreen" && lastGamemode == "game")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 350, 500, 140, "restart"));
            }
        }
        static void RenderScene()
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                Obstacle obstacle = obstacles[i];
                if (obstacle.type == "static")
                {
                    Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.DARKGRAY);
                }
                else if (obstacle.type == "editable")
                {
                    Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.DARKGREEN);
                }
                else if (obstacle.type == "goal")
                {
                    Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.ORANGE);
                }
            }
            Raylib.DrawRectangle((int)p1.x, (int)p1.y, (int)p1.width, (int)p1.height, Color.RED);
            if (drawing) DrawObstacle();
        }
        static void CheckKeyPresses()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) && (gameState == "game" || gameState == "create"))
            {
                lastGamemode = gameState;
                gameState = "pauseScreen";
                MenuButtons();
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W) && !Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                if (gameState == "game") Jump();
                else if (gameState == "create") yVelocity += (float)0.6;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S) && !Raylib.IsKeyDown(KeyboardKey.KEY_W) && gameState == "create") yVelocity -= (float)0.6;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A) && !Raylib.IsKeyDown(KeyboardKey.KEY_D)) xVelocity -= (float)0.6;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D) && !Raylib.IsKeyDown(KeyboardKey.KEY_A)) xVelocity += (float)0.6;
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                startingX = Raylib.GetMouseX() + x;
                startingY = Raylib.GetMouseY();
                drawing = true;
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON)) DeleteObstacle();
        }
        static void SaveStage()
        {
            currentLevel = Console.ReadLine();
            string[] files = Directory.GetFiles(@"saveFolder");
            levels.Clear();
            foreach (string line in files)
            {
                string name = line;
                name = name.Remove(name.Length - 4).Substring(11);
                levels.Add(name);
            }
            if (!(levels.Contains(currentLevel)))
            {
                levels.Add(currentLevel);
            }
            if (!(files.Contains(currentLevel + ".txt")))
            {
                File.Create(@"saveFolder/" + currentLevel + ".txt").Dispose();
            }
            using (var stream = File.Open(@"saveFolder/" + currentLevel + ".txt", FileMode.Open))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(stream))
                {
                    file.WriteLine("!" + currentLevel);
                    file.WriteLine("pX" + x);
                    file.WriteLine("pY" + p1.y);
                    for (int i = 0; i < obstacles.Count; i++)
                    {
                        Obstacle obstacle = obstacles[i];
                        file.WriteLine("x" + obstacle.x);
                        file.WriteLine("y" + obstacle.y);
                        file.WriteLine("w" + obstacle.width);
                        file.WriteLine("h" + obstacle.height);
                        file.WriteLine("t" + obstacle.type);
                    }
                }
            }
        }
        static void LoadStage()
        {
            obstacles.Clear();
            int tempX = 0;
            int tempY = 0;
            int tempWidth = 0;
            int tempHeight = 0;
            string tempType;
            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(@"saveFolder/" + currentLevel + ".txt");
            }
            catch (SystemException)
            {
                return;
            }
            foreach (string line in lines)
            {
                if (line.StartsWith("pX")) xStart = float.Parse(line.Substring(2));
                else if (line.StartsWith("pY")) yStart = float.Parse(line.Substring(2));
                else if (line.StartsWith("x")) tempX = int.Parse(line.Substring(1));
                else if (line.StartsWith("y")) tempY = int.Parse(line.Substring(1));
                else if (line.StartsWith("w")) tempWidth = int.Parse(line.Substring(1));
                else if (line.StartsWith("h")) tempHeight = int.Parse(line.Substring(1));
                else if (line.StartsWith("t"))
                {
                    tempType = line.Substring(1);
                    obstacles.Add(new Obstacle((int)tempX, (int)tempY, (int)tempWidth, (int)tempHeight, tempType));
                }
            }
        }
        static void DrawObstacle()
        {
            float xP1 = startingX;
            float yP1 = startingY;
            float xP2 = Raylib.GetMouseX() - startingX + x;
            float yP2 = Raylib.GetMouseY() - startingY;
            if (Raylib.GetMouseX() + x < startingX)
            {
                xP1 = Raylib.GetMouseX() + x;
                xP2 = startingX - xP1;
            }
            if (Raylib.GetMouseY() < startingY)
            {
                yP1 = Raylib.GetMouseY();
                yP2 = startingY - yP1;
            }
            if (xP1 - x < p1.x + p1.width && xP1 - x + xP2 > p1.x && yP1 + yP2 > p1.y && !(yP1 > p1.y + p1.height))
            {
                Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.RED);
            }
            else Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.BLUE);
            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
            {
                drawing = false;
                if (!(xP1 - x < p1.x + p1.width && xP1 - x + xP2 > p1.x && yP1 + yP2 > p1.y && !(yP1 > p1.y + p1.height)) && !(yP2 < 4 || xP2 < 4))
                {
                    if (gameState == "game") obstacles.Add(new Obstacle((int)xP1, (int)yP1, (int)xP2, (int)yP2, "editable"));
                    else if (gameState == "create") obstacles.Add(new Obstacle((int)xP1, (int)yP1, (int)xP2, (int)yP2, "static"));
                }
            }
        }
        static void DeleteObstacle()
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                Obstacle obstacle = obstacles[i];
                Rectangle r = new Rectangle(-x + obstacle.x, obstacle.y, obstacle.width, obstacle.height);
                if (Raylib.GetMouseX() > r.x && Raylib.GetMouseY() > r.y && Raylib.GetMouseX() < r.x + r.width && Raylib.GetMouseY() < r.y + r.height)
                {
                    if (gameState == "game" && obstacle.type == "editable") obstacles.RemoveAt(i);
                    else if (gameState == "create") obstacles.RemoveAt(i);
                }
            }
        }
        static void Jump()
        {
            if (touchingGround)
            {
                yVelocity = 9;
                touchingGround = false;
                jumping = true;
            }
        }
        static void UpdatePos()
        {
            if (jumping && gameState == "game")
            {
                if (yVelocity < 12) yVelocity += (float)0.5;
                else
                {
                    jumping = false;
                }
            }
            if (!touchingGround && !jumping && gameState == "game") yVelocity -= (float)0.5;
            p1.y -= yVelocity;
            x += xVelocity;
            xVelocity *= (float)0.9;
            if (gameState == "create") yVelocity *= (float)0.9;
        }
        static void RestartGameMode()
        {
            yVelocity = 0;
            xVelocity = 0;
            p1.y = (windowHeight / 4) * 3;
            x = 0;
            drawing = false;
        }
        static void CheckCollision()
        {
            if (p1.y > windowHeight)
            {
                RestartGameMode();
            }
            int nonTouchingObstacles = obstacles.Count;
            for (int i = 0; i < obstacles.Count; i++)
            {
                Obstacle obstacle = obstacles[i];
                Rectangle r2 = new Rectangle(-x + obstacle.x, obstacle.y, obstacle.width, obstacle.height);
                bool isOverlapping = Raylib.CheckCollisionRecs(p1, r2);
                if (isOverlapping && obstacle.type != "goal")
                {
                    if (x + p1.x <= x + r2.x && p1.y + p1.height + yVelocity > r2.y && p1.y < r2.y + r2.height - yVelocity)
                    {
                        x = r2.x + x - p1.x - p1.width;
                        xVelocity = 0;
                    }
                    if (p1.x + p1.width >= r2.x + r2.width && p1.y + p1.height + yVelocity > r2.y && p1.y < r2.y + r2.height - yVelocity)
                    {
                        x = r2.x + x + r2.width - p1.x;
                        xVelocity = 0;
                    }
                    if (p1.y + p1.height + yVelocity <= r2.y && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width)
                    {
                        touchingGround = true;
                        jumping = false;
                        p1.y = r2.y - p1.height;
                        yVelocity = 0;
                    }
                    if (p1.y + yVelocity >= r2.y + r2.height && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width)
                    {
                        jumping = false;
                        p1.y = r2.y + r2.height;
                        yVelocity = 0;
                    }
                }
                if (isOverlapping && obstacle.type == "goal")
                {
                    //implement a goal thingy
                }
                if (!(p1.y + p1.height == r2.y && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width) && !jumping)
                {
                    nonTouchingObstacles--;
                }
            }
            if (nonTouchingObstacles == 0) touchingGround = false;
        }
    }
    public class Obstacle
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public string type;
        public Obstacle(float x, float y, float width, float height, string type)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.type = type;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using Raylib_cs;
using System.Collections.Generic;

namespace Graphics
{
    public class Program
    {
        static int windowLength = 1900;
        static int windowHeight = 1000;
        static Rectangle p1 = new Rectangle(windowLength / 4, (windowHeight / 4) * 3, windowHeight / 29, windowHeight / 18);
        static List<Obstacle> obstacles = new List<Obstacle>();
        static List<Obstacle> buttons = new List<Obstacle>();
        static List<string> levels = new List<string>();
        static string currentLevel;
        static bool touchingGround;
        static bool jumping;
        static bool drawing = false;
        static float startingX;
        static float startingY;
        static int timer;
        static float x;
        static float xStart;
        static float yStart;
        static int yOffset = 0;
        static float xVelocity;
        static float yVelocity;
        static string gameState;
        static string lastGamemode;
        static bool allowBuild;
        static string buildType;
        static int sinceLastClick;
        static void Main(string[] args)
        {
            try
            {
                System.IO.Directory.CreateDirectory(@"saveFolder");
            }
            catch (SystemException)
            {
                throw;
            }
            string[] files = Directory.GetFiles(@"saveFolder");
            foreach (string line in files)
            {
                string name = line;
                name = ConvertName(name);
                levels.Add(name.ToLower());
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
                    if (gameState == "game")
                    {
                        CheckCollision();
                        timer++;
                    }
                }
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);
                if (gameState == "game" || gameState == "create" || gameState == "pauseScreen" || gameState == "goal")
                {
                    RenderScene();
                    StatusBar();
                }
                if (gameState == "pauseScreen" || gameState == "menu" || gameState == "levelSelect" || gameState == "gamemodeChoose") Menu();
                Raylib.EndDrawing();
            }
        }
        static void MenuInstrucions()
        {
            Raylib.DrawRectangle(windowLength / 8 - 30, windowHeight - 170, 1580, 220, Color.GRAY);
            Raylib.DrawText("Welcome to draw thingy", windowLength / 3, 20, 60, Color.BLACK);
            Raylib.DrawText("You can create a new level or open an existing one by clicking a button", windowLength / 8, windowHeight - 155, 40, Color.BLACK);
            Raylib.DrawText("Green obstacles can be placed down in both create mode and play mode", windowLength / 8, windowHeight - 120, 40, Color.BLACK);
            Raylib.DrawText("Red obstacles kills the player while in play mode, Orange is the goal", windowLength / 8, windowHeight - 85, 40, Color.BLACK);
            Raylib.DrawText("Once you're done with your level you can play it by saving it (IMPORTANT) and then accesing it from the levels menu", windowLength / 8, windowHeight - 42, 26, Color.BLACK);
        }
        static void Menu()
        {
            sinceLastClick++;
            if (gameState == "levelSelect")
            {
                if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) yOffset -= 4;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) yOffset += 4;
                if (yOffset < 0) yOffset = 0;
            }
            else yOffset = 0;
            drawing = false;
            for (int i = 0; i < windowHeight / 29; i++)
            {
                Raylib.DrawRectangle(0, i * 30, windowLength, 15, Color.LIGHTGRAY);
            }
            if (gameState == "levelSelect")
            {
                Raylib.DrawText("Use the down and up arrows to navigate", windowLength / 3 - 100, 5, 40, Color.DARKGRAY);
            }
            if (gameState == "menu") MenuInstrucions();
            for (int i = 0; i < buttons.Count; i++)
            {
                Obstacle button = buttons[i];
                Raylib.DrawRectangle((int)button.x, (int)button.y - yOffset, (int)button.width, (int)button.height, Color.DARKGRAY);
                Raylib.DrawRectangle((int)button.x + 10, (int)button.y + 10 - yOffset, (int)button.width - 20, (int)button.height - 20, Color.GRAY);
                Raylib.DrawText(button.type.ToUpper(), (int)(button.x + button.width / 10), (int)(button.y + button.height / 4) - yOffset, 80, Color.DARKGRAY);
                //remove button logic
                if (gameState == "levelSelect" && button.type != "back")
                {
                    Raylib.DrawRectangle((int)button.x + (int)button.width + 60, (int)button.y - yOffset, (int)(button.height * 2.25), (int)button.height, Color.RED);
                    Raylib.DrawText("remove level", (int)(button.x + button.width) + 80, (int)(button.y + button.height / 4) + 10 - yOffset, 40, Color.DARKGRAY);
                }
                if (Raylib.GetMouseX() > button.x + button.width + 60 && Raylib.GetMouseX() < button.x + button.width + 60 + button.height * 2.25 && Raylib.GetMouseY() > button.y - yOffset && Raylib.GetMouseY() < button.y + button.height - yOffset && button.type != "back" && gameState == "levelSelect")
                {
                    Raylib.DrawRectangle((int)button.x + (int)button.width + 60, (int)button.y - yOffset, (int)(button.height * 2.25), (int)button.height, Color.PINK);
                    Raylib.DrawText("remove level", (int)(button.x + button.width) + 80, (int)(button.y + button.height / 4) + 10 - yOffset, 40, Color.DARKGRAY);
                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                    {
                        string targetFile = button.type + ".txt";
                        try
                        {
                            File.Delete(@"saveFolder/" + targetFile);
                        }
                        catch (SystemException)
                        {
                            throw;
                        }
                        string[] files = Directory.GetFiles(@"saveFolder");
                        levels.Clear();
                        foreach (string line in files)
                        {
                            string name = line;
                            name = ConvertName(name);
                            levels.Add(name.ToLower());
                        }
                        MenuButtons();
                    }
                }
                //button logic
                if (Raylib.GetMouseX() > button.x && Raylib.GetMouseX() < button.x + button.width && Raylib.GetMouseY() > button.y - yOffset && Raylib.GetMouseY() < button.y + button.height - yOffset)
                {
                    Raylib.DrawRectangle((int)button.x + 10, (int)button.y + 10 - yOffset, (int)button.width - 20, (int)button.height - 20, Color.LIGHTGRAY);
                    Raylib.DrawText(button.type.ToUpper(), (int)(button.x + button.width / 10), (int)(button.y + button.height / 4) - yOffset, 80, Color.DARKGRAY);
                    if (!levels.Contains(currentLevel) && button.type == "save")
                    {
                        Raylib.DrawText("Open console application and enter name", (int)(button.x + 20), (int)button.y + ((int)button.height / 8) * 6 - yOffset, 22, Color.DARKGRAY);
                    }
                    if (!levels.Any() && button.type == "levels")
                    {
                        Raylib.DrawText("You have no saved levels", (int)(button.x + 20), (int)button.y + ((int)button.height / 8) * 6 - yOffset, 22, Color.DARKGRAY);
                    }
                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && sinceLastClick > 3)
                    {
                        sinceLastClick = 0;
                        if (button.type == "new")
                        {
                            allowBuild = true;
                            currentLevel = "";
                            obstacles.Clear();
                            x = 0;
                            gameState = "create";
                            buildType = "static";
                        }
                        else if (button.type == "levels" && levels.Any())
                        {
                            gameState = "levelSelect";
                            MenuButtons();
                        }
                        else if (button.type == "exit") Environment.Exit(0);
                        else if (button.type == "resume") gameState = lastGamemode;
                        else if (button.type == "save")
                        {
                            SaveStage();
                        }
                        else if (button.type == "restart")
                        {
                            x = xStart;
                            p1.y = yStart;
                            gameState = "game";
                            RestartGameMode();
                        }
                        else if (button.type == "menu")
                        {
                            gameState = "menu";
                            MenuButtons();
                        }
                        else if (levels.Contains(button.type))
                        {
                            currentLevel = button.type.ToLower();
                            gameState = "gamemodeChoose";
                            MenuButtons();
                        }
                        else if (button.type == "play")
                        {
                            gameState = "game";
                            buildType = "editable";
                            RestartGameMode();
                            LoadStage();
                        }
                        else if (button.type == "edit")
                        {
                            gameState = "create";
                            buildType = "static";
                            RestartGameMode();
                            LoadStage();
                        }
                        else if (button.type == "back")
                        {
                            if (gameState == "levelSelect") gameState = "menu";
                            else if (gameState == "gamemodeChoose") gameState = "levelSelect";
                            MenuButtons();
                        }
                    }
                }
            }
        }
        static void StatusBar()
        {
            if (allowBuild) Raylib.DrawText("Building on", 10, 10, 50, Color.DARKGRAY);
            else if (!allowBuild) Raylib.DrawText("Building off", 10, 10, 50, Color.DARKGRAY);
            if (gameState == "create")
            {
                Raylib.DrawText("Press (b) to toggle", 10, 60, 20, Color.DARKGRAY);
                Raylib.DrawText("Block type: " + buildType + " press (g) to change", windowLength / 3 - 20, 10, 40, Color.DARKGRAY);
            }
            else if (gameState == "game" || gameState == "goal")
            {
                Raylib.DrawText("Time: " + timer / 120, windowLength / 2 - 40, 10, 60, Color.DARKGRAY);
            }
            if (gameState == "goal")
            {
                Raylib.DrawRectangle(200, windowHeight / 2 - 140, windowLength - 400, 240, Color.DARKGRAY);
                Raylib.DrawRectangle(210, windowHeight / 2 - 130, windowLength - 420, 220, Color.GRAY);
                Raylib.DrawText("You beat the course with a time of " + timer / 120 + " seconds", 235, windowHeight / 2 - 80, 60, Color.LIGHTGRAY);
                Raylib.DrawText("Press \"Enter\" to continue...", 235, windowHeight / 2, 50, Color.LIGHTGRAY);
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                {
                    gameState = "menu";
                    MenuButtons();
                }
            }
        }
        static void MenuButtons()
        {
            buttons.Clear();
            if (gameState == "menu")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 130, 500, 140, "new"));
                buttons.Add(new Obstacle(windowLength / 2 - 250, 350, 500, 140, "levels"));
                buttons.Add(new Obstacle(windowLength / 2 - 250, 570, 500, 140, "exit"));
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
                    buttonName = ConvertName(buttonName);
                    buttons.Add(new Obstacle(windowLength / 2 - 250, 50 + i * 180, 500, 125, buttonName));
                }
            }
            else if (gameState == "gamemodeChoose")
            {
                buttons.Add(new Obstacle(windowLength / 2 - 250, 130, 500, 140, "play"));
                buttons.Add(new Obstacle(windowLength / 2 - 250, 350, 500, 140, "edit"));
            }
            if (gameState == "levelSelect" || gameState == "gamemodeChoose")
            {
                buttons.Add(new Obstacle(20, 60, 270, 140, "back"));

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
                    Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.GREEN);
                }
                else if (obstacle.type == "goal")
                {
                    Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.ORANGE);
                }
                else if (obstacle.type == "danger")
                {
                    Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.RED);
                }
            }
            Raylib.DrawRectangle((int)p1.x, (int)p1.y, (int)p1.width, (int)p1.height, Color.GOLD);
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
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_B) && gameState == "create") allowBuild = !allowBuild;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_G) && gameState == "create")
            {
                if (buildType == "editable") buildType = "static";
                else if (buildType == "static") buildType = "danger";
                else if (buildType == "danger") buildType = "goal";
                else if (buildType == "goal") buildType = "editable";
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S) && !Raylib.IsKeyDown(KeyboardKey.KEY_W) && gameState == "create") yVelocity -= (float)0.6;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A) && !Raylib.IsKeyDown(KeyboardKey.KEY_D)) xVelocity -= (float)0.6;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D) && !Raylib.IsKeyDown(KeyboardKey.KEY_A)) xVelocity += (float)0.6;
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && (gameState == "create" || allowBuild))
            {
                startingX = Raylib.GetMouseX() + x;
                startingY = Raylib.GetMouseY();
                drawing = true;
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON)) DeleteObstacle();
        }
        static void DrawObstacle()
        {
            float xP1 = startingX;
            float yP1 = startingY;
            float xP2 = Raylib.GetMouseX() - startingX + x;
            float yP2 = Raylib.GetMouseY() - startingY;
            //keep width and height positive
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
            //check if overlapping with player
            bool isOverlapping = xP1 - x < p1.x + p1.width && xP1 - x + xP2 > p1.x && yP1 + yP2 > p1.y && !(yP1 > p1.y + p1.height);
            if (isOverlapping && gameState == "game")
            {
                Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.RED);
            }
            else Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.BLUE);
            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
            {
                drawing = false;
                if ((!isOverlapping || gameState == "create") && !(yP2 < 5 || xP2 < 5))
                {
                    obstacles.Add(new Obstacle((int)xP1, (int)yP1, (int)xP2, (int)yP2, buildType));
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
            if (jumping)
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
            LoadStage();
            yVelocity = 0;
            xVelocity = 0;
            x = xStart;
            p1.y = yStart;
            drawing = false;
            timer = 0;
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
                if (isOverlapping && obstacle.type != "goal" && obstacle.type != "danger")
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
                    gameState = "goal";
                }
                if (isOverlapping && obstacle.type == "danger")
                {
                    RestartGameMode();
                }
                if (!(p1.y + p1.height == r2.y && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width) && !jumping)
                {
                    nonTouchingObstacles--;
                }
            }
            if (nonTouchingObstacles == 0) touchingGround = false;
        }
        static void SaveStage()
        {
            string[] files = Directory.GetFiles(@"saveFolder");
            levels.Clear();
            foreach (string line in files)
            {
                string name = line;
                name = ConvertName(name);
                levels.Add(name.ToLower());
            }
            if (!(levels.Contains(currentLevel)))
            {
                Console.Write("\nPlease enter the name for your level here: ");
                string response = Console.ReadLine();
                while (response == "" || response.Length > 10 || levels.Contains(response.ToLower()))
                {
                    Console.WriteLine("\nName cannot be longer than 10 characters or be an already existing name");
                    Console.Write("Please enter the name for your level here:");
                    response = Console.ReadLine();
                }
                currentLevel = response.ToLower();
                levels.Add(currentLevel);
                Console.WriteLine("Thank you! You may now return to the game");
            }
            if (!(files.Contains(currentLevel + ".txt")))
            {
                File.Create(@"saveFolder/" + currentLevel + ".txt").Dispose();
            }
            using (var stream = File.Open(@"saveFolder/" + currentLevel + ".txt", FileMode.Open))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(stream))
                {
                    file.WriteLine("?" + allowBuild);
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
                throw;
            }
            foreach (string line in lines)
            {
                if (line.StartsWith("?")) allowBuild = bool.Parse(line.Substring(1));
                else if (line.StartsWith("pX")) xStart = float.Parse(line.Substring(2));
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
            x = xStart;
            p1.y = yStart;
        }
        static string ConvertName(string text)
        {
            text = text.Remove(text.Length - 4).Substring(11);
            return text;
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

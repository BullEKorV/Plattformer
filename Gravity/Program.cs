using System;
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
        static bool touchingGround;
        static bool jumping;
        static bool drawing = false;
        static float startingX;
        static float startingY;
        static float x = 0;
        static float xVelocity;
        static float yVelocity;
        static void Main(string[] args)
        {
            ObstacleSetup();
            Raylib.SetTargetFPS(120);
            Raylib.InitWindow(windowLength, windowHeight, "Test");
            while (!Raylib.WindowShouldClose())
            {
                CheckKeyPresses();
                UpdatePos();
                CheckCollision();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);
                Raylib.DrawRectangle((int)p1.x, (int)p1.y, (int)p1.width, (int)p1.height, Color.RED);
                for (int i = 0; i < obstacles.Count; i++)
                {
                    Obstacle obstacle = obstacles[i];
                    if (!obstacle.editable)
                    {
                        Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.DARKGRAY);
                    }
                    else
                    {
                        Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, Color.DARKGREEN);
                    }
                }
                if (drawing) DrawObstacle();
                Raylib.EndDrawing();
            }
        }
        static void ObstacleSetup()
        {
            obstacles.Add(new Obstacle(300, windowHeight - 300, windowLength - 600, 200, false));
            obstacles.Add(new Obstacle(200, windowHeight - 100, windowLength - 400, 100, false));
            obstacles.Add(new Obstacle(0, windowHeight - 10, windowLength, 10, false));
            obstacles.Add(new Obstacle(0, (windowHeight / 3) * 2, 150, 30, false));
            obstacles.Add(new Obstacle(0, (windowHeight / 2) - 20, 150, 30, false));
        }
        static void CheckKeyPresses()
        {
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W) && !Raylib.IsKeyDown(KeyboardKey.KEY_S)) Jump();
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S) && !Raylib.IsKeyDown(KeyboardKey.KEY_W)) yVelocity = (int)3;
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
            //if (startingX - x < p1.x + p1.width && startingX - x + xP2 > p1.x && startingY <= p1.y + p1.height && !(yP2 < p1.y + p1.height)) startingY = p1.y + p1.height;
            //if (yP1 < p1.y + p1.height && yP1 + p1.height > p1.y && Raylib.GetMouseX() + x >= p1.x) xP2 = p1.x - xP1;
            if (xP1 - x < p1.x + p1.width && xP1 - x + xP2 > p1.x && yP1 + yP2 > p1.y && !(yP1 > p1.y + p1.height))
            {
                Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.RED);
            }
            else Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.BLUE);
            Console.WriteLine(yP1 + "    " + xP1 + "    " + yP2 + "     " + xP2);
            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
            {
                drawing = false;
                if (!(xP1 - x < p1.x + p1.width && xP1 - x + xP2 > p1.x && yP1 + yP2 > p1.y && !(yP1 > p1.y + p1.height)))
                {
                    obstacles.Add(new Obstacle((int)xP1, (int)yP1, (int)xP2, (int)yP2, true));
                }
            }
        }
        static void DeleteObstacle()
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                Obstacle obstacle = obstacles[i];
                Rectangle r = new Rectangle(-x + obstacle.x, obstacle.y, obstacle.width, obstacle.height);
                if (Raylib.GetMouseX() > r.x && Raylib.GetMouseY() > r.y && Raylib.GetMouseX() < r.x + r.width && Raylib.GetMouseY() < r.y + r.height && obstacle.editable)
                {
                    obstacles.RemoveAt(i);
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
            if (!touchingGround && !jumping) yVelocity -= (float)0.5;
            p1.y -= yVelocity;
            x += xVelocity;
            xVelocity *= (float)0.9;
        }
        static void CheckCollision()
        {
            if (p1.y > windowHeight)
            {
                yVelocity = 0;
                xVelocity = 0;
                p1.y = (windowHeight / 4) * 3;
                x = 0;
                drawing = false;
            }
            int nonTouchingObstacles = obstacles.Count;
            for (int i = 0; i < obstacles.Count; i++)
            {
                Obstacle obstacle = obstacles[i];
                Rectangle r2 = new Rectangle(-x + obstacle.x, obstacle.y, obstacle.width, obstacle.height);
                bool isOverlapping = Raylib.CheckCollisionRecs(p1, r2);
                if (isOverlapping)
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
                if (!(p1.y + p1.height == r2.y && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width) && !jumping)
                {
                    nonTouchingObstacles--;
                }
            }
            if (nonTouchingObstacles == 0) touchingGround = false;
            Console.WriteLine(nonTouchingObstacles);
        }
    }
    public class Obstacle
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public bool editable;
        public Obstacle(float x, float y, float width, float height, bool editable)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.editable = editable;
        }
    }
}

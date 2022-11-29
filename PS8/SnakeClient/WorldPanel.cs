/**
 * 2022-11-23 
 *      - Add WallDrawer method. Calculate number of walls by using Point1 and 2.
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using GameWorld;
using Microsoft.UI.Xaml.Controls;
using System.Xml;
using Microsoft.Maui.Graphics;

namespace SnakeGame;
public class WorldPanel : IDrawable
{
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    private GraphicsView graphicsView = new();

    private int viewSize = 900;
    private World theWorld;
    private int playerUniqueID = -1;

    private IImage wall;
    private IImage background;

    private bool initializedForDrawing = false;
    private bool isSnakeDied = false;

#if MACCATALYST
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#endif

    public WorldPanel()
    {
        graphicsView.Drawable = this;
        graphicsView.HeightRequest = 900;
        graphicsView.WidthRequest = 900;
    }

    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double dir, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        //canvas.Rotate((float)dir);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    private void SnakeDrawer(object o, ICanvas canvas)
    {
        Snake s = o as Snake;

        if (!isSnakeDied)
        {
            int lengthOfSnake = s.Body.Count;

            switch (s.UniqueID % 8)
            {
                case 0:
                    canvas.StrokeColor = Colors.DarkRed;
                    break;
                case 1:
                    canvas.StrokeColor = Colors.DarkGreen;
                    break;
                case 2:
                    canvas.StrokeColor = Colors.Yellow;
                    break;
                case 3:
                    canvas.StrokeColor = Colors.Black;
                    break;
                case 4:
                    canvas.StrokeColor = Colors.DarkOrange;
                    break;
                case 5:
                    canvas.StrokeColor = Colors.Blue;
                    break;
                case 6:
                    canvas.StrokeColor = Colors.Brown;
                    break;
                case 7:
                    canvas.StrokeColor = Colors.AliceBlue;
                    break;
            }

            canvas.StrokeSize = 10;
            canvas.StrokeLineCap = LineCap.Round;


            float firstX = 0;
            float firstY = 0;
            float secondX = 0;
            float secondY = 0;
            Vector2D temp = s.Body.Last();

            canvas.FontColor = Colors.White;

            for (int i = lengthOfSnake - 1; i > 0; i--)
            {
                if (temp.X == s.Body[i - 1].X)
                    secondY += (float)(s.Body[i - 1].Y - temp.Y);
                else if (temp.Y == s.Body[i - 1].Y)
                    secondX += (float)(s.Body[i - 1].X - temp.X);

                canvas.DrawLine(firstX, firstY, secondX, secondY);

                firstX = secondX;
                firstY = secondY;
                temp = s.Body[i - 1];
            }
            canvas.DrawString(s.Name + ": " + s.Score, 0, 20, HorizontalAlignment.Center);
        }
        else
        {
            Random rand = new Random();
            int explosionValueX = rand.Next(20);
            int explosionValueY = rand.Next(20);
            float randRotate = rand.Next(360);

            canvas.FillColor = Colors.WhiteSmoke;

            for (int i = 0; i < 3; i++)
            {
                canvas.FillCircle((float)(0 + explosionValueX), (float)(0 + explosionValueY), 4);
                canvas.Rotate((float)randRotate);
            }
        }

        canvas.Rotate((float)s.Dir.ToAngle());
        canvas.FillColor = Colors.White;
        canvas.FillCircle(5, 1, 3);
        canvas.FillCircle(-5, 1, 3);
        canvas.FillColor = Colors.Black;
        canvas.FillCircle(5, 0, 2);
        canvas.FillCircle(-5, 0, 2);
    }
    //private void SnakeDieDrawer(object o, ICanvas canvas)
    //{
    //    Snake s = o as Snake;

    //    canvas.FillColor = Colors.WhiteSmoke;
    //    float randRotate = (float)((s.Body.Last().GetX() + s.Body.Last().GetY()) % 360);

    //    if (movingCount <= 35)
    //    {
    //        for (int i = 0; i < 8; i++)
    //        {
    //            canvas.DrawCircle((float)(0 + movingCount), 0, 4);
    //            canvas.Rotate((float)randRotate * (i + 5));
    //        }
    //    }
    //    movingCount += 1;
    //}

    private void WallsDrawer(object o, ICanvas canvas)
    {
        Wall w = o as Wall;

        double x1 = w.Point1.GetX();
        double y1 = w.Point1.GetY();
        double x2 = w.Point2.GetX();
        double y2 = w.Point2.GetY();

        float width = 50;
        float height = 50;

        double lengthOfWallInRow = x1 - x2;
        double lengthOfWallInCol = y1 - y2;

        int numOfRow = (int)(lengthOfWallInRow / width);
        int numOfCol = (int)(lengthOfWallInCol / width);

        if (numOfRow < 0)
            numOfRow *= -1;

        if (numOfCol < 0)
            numOfCol *= -1;

        if (lengthOfWallInRow > 0 && lengthOfWallInCol == 0)
        {
            for (int i = 0; i <= numOfRow; i++)
            {
                canvas.DrawImage(wall, (-width / 2) - (i * width), -height / 2, width, height);
            }
        }
        else if (lengthOfWallInRow < 0 && lengthOfWallInCol == 0)
        {
            for (int i = 0; i <= numOfRow; i++)
            {
                canvas.DrawImage(wall, (-width / 2) + (i * width), -height / 2, width, height);
            }
        }

        else if (lengthOfWallInCol > 0 && lengthOfWallInRow == 0)
        {
            for (int i = 0; i <= numOfCol; i++)
            {
                canvas.DrawImage(wall, (-width / 2), (-height / 2) - (i * height), width, height);
            }
        }
        else if (lengthOfWallInCol < 0 && lengthOfWallInRow == 0)
        {
            for (int i = 0; i <= numOfCol; i++)
            {
                canvas.DrawImage(wall, (-width / 2), (-height / 2) + (i * height), width, height);
            }
        }
    }

    private void PowerUpDrawer(object o, ICanvas canvas)
    {
        PowerUp p = o as PowerUp;
        float radius1 = 8;
        float radius2 = 4;

        canvas.FillColor = Colors.DarkGreen;
        canvas.FillCircle(0, 0, radius1);

        canvas.FillColor = Colors.Yellow;
        canvas.FillCircle(0, 0, radius2);
    }

    public void SetWorld(World w)
    {
        theWorld = w;
    }

    public void SetUniqueID(int id)
    {
        playerUniqueID = id;
    }

    private void InitializeDrawing()
    {
        wall = loadImage("WallSprite.png");
        background = loadImage("Background.png");
        initializedForDrawing = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
            InitializeDrawing();

        canvas.ResetState();

        if (theWorld is not null)
        {

            if (theWorld.SnakePlayers.ContainsKey(playerUniqueID))
            {
                
                    float playerX = (float)theWorld.SnakePlayers[playerUniqueID].Body.Last().GetX();
                    float playerY = (float)theWorld.SnakePlayers[playerUniqueID].Body.Last().GetY();

                    canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

                    canvas.DrawImage(background, (-theWorld.Size) / 2, (-theWorld.Size) / 2, theWorld.Size, theWorld.Size);

                lock (theWorld)
                {
                    //Drawing walls
                    foreach (Wall w in theWorld.Walls.Values.ToList())
                        DrawObjectWithTransform(canvas, w, w.Point1.GetX(), w.Point1.GetY(), 0, WallsDrawer);

                    // Even if snake isn't disappear after colliding wall, add if(!s.Died)
                    foreach (Snake s in theWorld.SnakePlayers.Values.ToList())
                    {
                        if (s.Died) { isSnakeDied = true; }
                        
                        if (!s.Alive) { isSnakeDied = true; }
                        else { isSnakeDied = false; }

                        DrawObjectWithTransform(canvas, s, s.Body.Last().GetX(), s.Body.Last().GetY(), 0, SnakeDrawer);
                    }

                    foreach (PowerUp p in theWorld.PowerUps.Values.ToList())
                        DrawObjectWithTransform(canvas, p, p.Location.GetX(), p.Location.GetY(), 0, PowerUpDrawer);
                }
            }
        }
    }
}
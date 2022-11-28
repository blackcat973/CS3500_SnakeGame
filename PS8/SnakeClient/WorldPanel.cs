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
    private double movingCount = 0;

    private IImage wall;
    private IImage background;

    private bool initializedForDrawing = false;

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
        int lengthOfSnake = s.Body.Count;
        if (s.UniqueID == playerUniqueID)
            canvas.StrokeColor = Colors.DarkBlue;
        else
            canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 10;
        canvas.StrokeLineCap = LineCap.Round;

        float firstX = 0;
        float firstY = 0;
        float secondX = 0;
        float secondY = 0;
        Vector2D temp = s.Body.Last();

        canvas.FontColor = Colors.White;

        if (s.Alive)
        {
            movingCount = 0;
            for (int i = lengthOfSnake - 1; i > 0; i--)
            {
                if (temp.X == s.Body[i - 1].X)
                    secondY += (float)(s.Body[i - 1].Y - temp.Y);
                else if (temp.Y == s.Body[i - 1].Y)
                    secondX += (float)(s.Body[i - 1].X - temp.X);

                canvas.DrawLine(firstX, firstY, secondX, secondY);
                //canvas.StrokeColor = Colors.WhiteSmoke;
                //canvas.StrokeDashPattern = new float[] { 3, 3 };
                //canvas.DrawLine(firstX, firstY, secondX, secondY);

                firstX = secondX;
                firstY = secondY;
                temp = s.Body[i - 1];
            }
            canvas.DrawString(s.Name + ": " + s.Score, 0, 20, HorizontalAlignment.Center);
        }
    }

    private void SnakeDieDrawer(object o, ICanvas canvas)
    {
        Snake s = o as Snake;
        canvas.FillColor = Colors.WhiteSmoke;
        float randRotate = (float)((s.Body.Last().GetX() + s.Body.Last().GetY()) % 360);

        if (movingCount <= 35)
        {
            for (int i = 0; i < 8; i++)
            {
                canvas.DrawCircle((float)(0 + movingCount), 0, 4);
                canvas.Rotate((float)randRotate * (i + 1));
            }
        }

        movingCount += 1;
    }

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
                    foreach (Wall w in theWorld.Walls.Values)
                        DrawObjectWithTransform(canvas, w, w.Point1.GetX(), w.Point1.GetY(), 0, WallsDrawer);

                    // Even if snake isn't disappear after colliding wall, add if(!s.Died)
                    foreach (Snake s in theWorld.SnakePlayers.Values)
                    {
                        if (s.Alive)
                            DrawObjectWithTransform(canvas, s, s.Body.Last().GetX(), s.Body.Last().GetY(), 0, SnakeDrawer);
                        else
                            DrawObjectWithTransform(canvas, s, s.Body.Last().GetX(), s.Body.Last().GetY(), 0, SnakeDieDrawer);
                    }

                    foreach (PowerUp p in theWorld.PowerUps.Values)
                        DrawObjectWithTransform(canvas, p, p.Location.GetX(), p.Location.GetY(), 0, PowerUpDrawer);
                }
            }
        }
    }
}
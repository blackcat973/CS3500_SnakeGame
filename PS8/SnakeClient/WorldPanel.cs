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
        canvas.Rotate((float)dir);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    private void SnakeDrawer(object o, ICanvas canvas)
    {
        Snake s = o as Snake;

        int lengthOfSnake = s.Body.Count;

        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 6;
        canvas.StrokeLineCap = LineCap.Round;

        //canvas.DrawLine(0, 0, 0, (float)lengthOfSnake);

        Vector2D temp = s.Body.Last();

        for (int i = lengthOfSnake - 1; i > 0; i--)
        {
            float HeadX = (float)(s.Body[i].X - temp.X);
            float HeadY = (float)(s.Body[i].Y - temp.Y);
            float TailX = (float)(s.Body[i - 1].X - temp.X);
            float TailY = (float)(s.Body[i - 1].Y - temp.Y);

            canvas.DrawLine(HeadX, HeadY, TailX, TailY);

            temp.X = HeadX;
            temp.Y = HeadY;
        }
    }

    private void WallsDrawer(object o, ICanvas canvas)
    {
        Wall w = o as Wall;

        double x1 = w.Point1.GetX();
        double y1 = w.Point1.GetY();
        double x2 = w.Point2.GetX();
        double y2 = w.Point2.GetY();

        double lengthOfWallInRow = x1 - x2;
        double lengthOfWallInCol = y1 - y2;

        int numOfRow = (int)lengthOfWallInRow / (int)wall.Width;
        int numOfCol = (int)lengthOfWallInCol / (int)wall.Width;

        if (numOfRow < 0)
            numOfRow *= -1;

        if (numOfCol < 0)
            numOfCol *= -1;

        if (lengthOfWallInRow > 0 && lengthOfWallInCol == 0)
        {
            for (int i = 0; i <= numOfRow; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2) - (i * wall.Width), -wall.Height / 2, wall.Width, wall.Height);
            }
        }
        else if (lengthOfWallInRow < 0 && lengthOfWallInCol == 0)
        {
            for (int i = 0; i <= numOfRow; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2) + (i * wall.Width), -wall.Height / 2, wall.Width, wall.Height);
            }
        }


        else if (lengthOfWallInCol > 0 && lengthOfWallInRow == 0)
        {
            for (int i = 0; i <= numOfCol; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2), (-wall.Height / 2) - (i * wall.Width), wall.Width, wall.Height);
            }
        }
        else if (lengthOfWallInCol < 0 && lengthOfWallInRow == 0)
        {
            for (int i = 0; i <= numOfCol; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2), (-wall.Height / 2) + (i * wall.Width), wall.Width, wall.Height);
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
                lock (theWorld)
                {
                    float playerX = (float)theWorld.SnakePlayers[playerUniqueID].Body.Last().GetX();
                    float playerY = (float)theWorld.SnakePlayers[playerUniqueID].Body.Last().GetY();

                    canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

                    canvas.DrawImage(background, (-theWorld.Size) / 2, (-theWorld.Size) / 2, theWorld.Size, theWorld.Size);

                    //Drawing walls
                    foreach (Wall w in theWorld.Walls.Values)
                        DrawObjectWithTransform(canvas, w, w.Point1.GetX(), w.Point1.GetY(), 0, WallsDrawer);

                    // Even if snake isn't disappear after colliding wall, add if(!s.Died)
                    foreach (Snake s in theWorld.SnakePlayers.Values)
                        DrawObjectWithTransform(canvas, s, s.Body.First().GetX(), s.Body.First().GetY(), s.Dir.ToAngle(), SnakeDrawer);

                    foreach (PowerUp p in theWorld.PowerUps.Values)
                        DrawObjectWithTransform(canvas, p, p.Location.GetX(), p.Location.GetY(), 0, PowerUpDrawer);
                } 
            }
        }
    }
}
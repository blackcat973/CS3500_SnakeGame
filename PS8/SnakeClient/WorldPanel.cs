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

namespace SnakeGame;
public class WorldPanel : IDrawable
{
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    private GraphicsView graphicsView = new();

    private int viewSize = 900;
    private World theWorld;

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
  private IImage loadImage( string name )
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream( assembly.GetManifestResourceStream( $"{path}.{name}" ) );
    }
#endif

    public WorldPanel()
    {
        graphicsView.Drawable = this;
        graphicsView.HeightRequest = 900;
        graphicsView.WidthRequest = 900;
    }
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    private void BackgroundDrawer(object o, ICanvas canvas)
    {
        canvas.DrawImage(background, -2000 / 2, -2000 / 2, 2000, 2000);
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

        if(numOfRow <0)
            numOfRow *= -1;

        if(numOfCol <0)
            numOfCol *= -1;

        if (lengthOfWallInRow > 0 && lengthOfWallInCol == 0)
        {
            for (int i = 0; i <= numOfRow; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2) - (i*wall.Width), -wall.Height / 2, wall.Width, wall.Height);
            }
        }
        else if (lengthOfWallInRow < 0 && lengthOfWallInCol == 0)
        {
            for (int i = 0; i <= numOfRow; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2) + (i*wall.Width), -wall.Height / 2, wall.Width, wall.Height);

            }
        }
        else if (lengthOfWallInCol > 0 && lengthOfWallInRow == 0)
        {
            for (int i = 0; i <= numOfCol; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2), (-wall.Height / 2) - (i*wall.Width), wall.Width, wall.Height);
            }
        }
        else if (lengthOfWallInCol < 0 && lengthOfWallInRow == 0)
        {
            for (int i = 0; i <= numOfCol; i++)
            {
                canvas.DrawImage(wall, (-wall.Width / 2), (-wall.Height / 2) + (i*wall.Width), wall.Width, wall.Height);
            }
        }
    }

    public void SetWorld(World w)
    {
        theWorld = w;
    }

    private void InitializeDrawing()
    {
        wall = loadImage( "WallSprite.png" );
        background = loadImage( "Background.png" );
        initializedForDrawing = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        canvas.Translate((float)viewSize / 2, (float)viewSize / 2);
        lock (theWorld)
        {
            DrawObjectWithTransform(canvas, background, 0, 0, BackgroundDrawer);

            foreach (var w in theWorld.Walls.Values)
                DrawObjectWithTransform(canvas, w, w.Point1.GetX(), w.Point1.GetY(), WallsDrawer);
        }
        

        
        canvas.DrawImage(wall, 0, 0, wall.Width, wall.Height);
        //canvas.DrawImage(wall, 0+wall.Width, 0, wall.Width, wall.Height);
        //canvas.DrawImage(wall, 0, 0+wall.Height, wall.Width, wall.Height);

        //WallsDrawer(canvas);
    }
}

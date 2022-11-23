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
        
        canvas.DrawImage(background, 0, 0, 900, 900);
        canvas.DrawImage(wall, 0, 0, wall.Width, wall.Height);
    }
}

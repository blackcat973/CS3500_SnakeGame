﻿using GameSystem;

namespace SnakeGame;


public partial class MainPage : ContentPage
{

    private GameController gameController = new();

    public MainPage()
    {

        InitializeComponent();
        /// Use this method as an event handler for when the controller has updated the world
        gameController.DatasArrived += OnFrame;

        gameController.WorldCreate += drawingWorld;

    }

    private void drawingWorld()
    {
        worldPanel.SetWorld(gameController.World);

        OnFrame();
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            gameController.InputKey("up");
        }
        else if (text == "a")
        {
            gameController.InputKey("left");
        }
        else if (text == "s")
        {
            gameController.InputKey("down");
        }
        else if (text == "d")
        {
            gameController.InputKey("right");
        }
        else
        {
            gameController.InputKey("none");
        }
        entry.Text = "";
    }

    private void NetworkErrorHandler()
    {
        DisplayAlert("Error", "Disconnected from server", "OK");
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt logic here in the view, instead of the controller,
    /// because it is closely tied with disabling/enabling buttons, and showing dialogs.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }
        //DisplayAlert("Delete this", "Code to connect to server goes here", "OK");

        gameController.Connect(serverText.Text, nameText.Text);

        gameController.InfoEntered(nameText.Text);

        keyboardHack.Focus();


    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch( () => graphicsView.Invalidate() );
    }

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by ...\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }
}
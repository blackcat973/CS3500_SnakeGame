# PS8_SnakeGame

This is the first version of the program that implements a Controller and View of the snake game. This program allows you to play 'simple snake game' through MAUI application.

## Features

- We added the cute eyes on the snake
- We added the explosion and it will spread out randomly but not too widely 
- Server Status visible 

### Model

Snakes, PowerUp, and Walls are based on the instruction, we added the default Constructions. The world class will contain Snake(Player), Wall, and PowerUp. We store those data into Dictionary since each object has a Specific Key and in this case, we can access the data fastly.

### View

1. WorldPanel

We drew canvas using the DrawObjectWithTransform method as much as possible. This is because it was much easier to calculate by moving the location of the canvas of the object than by calculating the world’s coordinates. Instead, if we need to figure and draw coordinates in a row, like the body of a snake, we used two body coordinates which is the world coordinates, and subtract them, and save the result to match the object coordinates to DrawLine coordinates.

You can see that we used foreach with ‘ToList()’ for drawing objects. This is because even though we lock the state to get every data of the world, if Enumerator was modified, it threw exception, called ‘System.InvalidOperationException’, because it was modified. For this reason, we added .ToList() and we can avoid the Exception. The specific information of the exception is below.

'''
System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
'''

-	GraphicView: First, the center of the graphicView, which is the screen that the user actually watches, is the coordinate of the snake’s head. Through this, we can make the screen move to match where the snake is going. By drawing the background according to the world coordinates, the world did not be drag along even if the graphicView moved.

-	Snake: We drew the body based on the coordinates of the snake’s head. Also, when drawing the body, we calculated the coordinates of the two attached index body so that we can draw snake properly with using DrawLine method. Therefore, when the head of the body moves, the tail can be drawn according to the coordinates of the head of the body. In the event of Died, the circle appeared randomly, creating the shape of a firework. It also kept the afterimage long enough to make notice that other users knew that the snake was died there.

-	Wall: X and Y coordinates of starting point and end point were calculated, and we divided the result by the length of one side of the wall so that we can get the number of walls. And then we drew walls according to the point coordinates based on the number of walls.

-	PowerUp: Like snake, we transformed the world coordinates into object coordinates, and then made them possible to be drawn there easily.

2. MainPage.xaml.cs

We used four handlers informed by the Controller, and it allowed us to update the View.

-	OnFrame: This is a method that draws a canvas in every single frame. Once the updated data is sent to View, View will draw objects on canvas through invalidate() based on that updated information.

-	DrawingWorld: This method is updated when WorldCreate Handler is invoked. It initializes the World of the game where user actually would play. 

-	NetworkErrorHandler: This method is updated when a network-related error occurs. If the Controller encounters a network-related error, for example, a socket, it notifies the user that a network-related error has occurred during the game. Also, when this method is updated, it sets the status light informing you of the status of your network connection to red. At this point, activate the Connect button to allow the user trying to reconnect to the server again.

-	ButtonDisable: This method prevents errors from occurring by clicking the Connect button infinitely when connected to the server. If you are connected to the server, the Connect button changes to disable to prevent unexpected errors in advance. Also, since the connection is activated, the connection status light will turn green.

### Controller

- Connection: When the user presses the Connect button in the View parts, the Controller will receive the player name, and IP address, and make a connection to the server. If we connect to the server, Connected will be invoked and let the view knows the connection is successful. After that, we will disable the ConnectButton to prevent the weird case that users press the button while they are playing. Also, change the ServerStatus from Red to Green to notice that the Server is working fine. 

- Connection Problem: Whenever the state’s ErrorOccurred is true. It means that the network has some problem such as an invalid IP Address, disconnect, or the server is closed. In this case, we invoke the Error to let View knows there is some problem with the server. Whenever the Error is invoked, it will enable the connect button again to let the user re-connect and the ServerStatus color will change from Green to Red to notice. 

- Handling the Data: We created the ParsingData() method for parsing the data since we get data from the server as JSON type and we need to store data into World class (Model). When data is split and called in foreach loop, we decided to try to parse each data. If so, we do not need any other complexity for parsing data. When we tried to Parse the data and if it is not JSON type, we can tell there are 3 cases which are UniqueId, WorldSize, or wrong data format from the server. However, we can guarantee that the first data is the Unique ID for the user and the second data is world size. When we get world size, it means we are allowed to create the World. We invoked the WorldCreate and let view draws the world. After storing the all of data from the server, we invoke the DatasArrived to let view know. Whenever DatasArrived is invoked, View will draw Snakes, PowerUp, and Walls based on the World data. 

- InputKey: Whenever the user presses the key, the TextChanged event will fire in the View parts and send to the Controller what the user pressed. We should not send any command requests to the server before receiving the player ID, world size, and Walls. Therefore, we used the if statement to check if World have received the Snake or PowerUp (because they will only come after Wall). Also, while the network ErrorOccurred is true, if the user try to press the key, we will alert that the Connection is lost.

- Race Condition: We lock the state when we call the ProcessData() method. While we are parsing the data and drawing based on the Wolrd’s information, if we do not lock the state, there is a possibility that Snakes’ or PowerUps’ Dictionary (in the world) can be modified while WorldPanel class draws the snake or powerUps. Therefore, we lock the “state” for ProcessData() method and lock the “theWorld” in the WorldPanel class.

**Part 3 – Views**

When games have large rooms that cannot be shown in their entirety on the screen, they only display a partial area of the room and move that area when needed. Like most game engines, ArcadeMaker has a built-in feature for this, called a **View** (also commonly called a "camera"). Every room can have multiple views and define each view's port (the area on the screen where the view is displayed), its position and size, and the area of the room that it should display. Views can be used for scaling (showing a large part of the room inside a small port, or the opposite), they can be moved, and they provide built-in functionality for following a specific object.

In this part, we'll split our tank object into two players, with each player having their own view following them.

**Making a view for the single-tank**

Before we split our tank into two players, we'll first create a single view that follows it. Open our room's editor and go to the **Settings** tab. Set the room size to, say, 2000x2000. If we ran the game now, a 2000x2000 (probably larger than your screen...) window would open, because unless we set at least one view, the window size will match the room size.

So, go to the **Views** tab, select the first view in the list, and check the **Visible when room starts** checkbox. You can see that the default view size and port size are 640x480, which is also the default size for a new room. Keep it like this for now and run the game.

A 640x480 window should open, showing the (0, 0)-(640, 480) area of the room. However, if you move the tank, the view does not follow it because we haven't programmed it to. Close the game and, in the view settings, under **Object following**, select **obj_tank** and set the **HBor** (horizontal border) to half of our view's width, which is 320, and the **VBor** (vertical border) to half of our view's height, which is 240.

This will make the view keep the player in the center, unless doing so would make the view go outside the room's bounds. Run the game again: the view should now follow the tank.

**Two heads are better than one**

Let's make our **obj_tank** have two instances, one for each player. Go back to the **Objects** tab in the room editor, select **obj_tank**, and place another one near the existing one.

Then go back to the **Views** tab, select the first view, and set its size and port size to 350x480. Now select the second view in the list, enable it by checking the **Visible when room starts** checkbox, and set its size and port size to 350x480 as well.

For this view, we also need to set a different **port x**: the width of view 1 plus a little spacing, so set it to 352.

In the **Backgrounds** tab, set the room backcolor to black, so the view splitter will be black instead of the default ugly gray. Don't forget to make view 2 follow **obj_tank** as well. We'll need to define the specific instance that each view follows; we'll do that in the code in a moment.

Close the room editor, open the **obj_tank** editor, and then open the **spr_tank** editor. Click Edit, and in the window that opens, click the "Load" icon and select the "tank_green" sprite from the sprites directory.

Now the sprite has two subimages. Multiple subimages can be used for animated sprites or for easily switching between sprites, like in our case, where we just want each of our tanks to have a different color.

Currently, both tanks are controlled using the same keyboard keys. We need to create mutable properties for the control keys and assign different values to them for each player.

On the right side of the object editor, there's a panel for editing the object's properties (we've already created some). Using the **Add** button in this panel, create five new properties: **k_right, k_left, k_up, k_down,** and **k_shoot**. Set their type to **Number** and assign a different key value to each property.

Also, add a property named player1 with type **bool**. We'll use this whenever we need to determine whether a tank is player 1 or player 2. Set its value to **null**! (and check the **Nullable** checkbox). We'll assign its value in the code.

And one last property for now: **otherPlayer**. Set its type to **Any** and its value to **Null**. We'll keep a reference to the other player in this property.

Right before we initialize all these properties, it's time to add a class for some general static properties that we'll access from different places in the game.

Right-click the **Scripts** directory in the project tree and select **Create Script**. In the code editor that opens, put this code:

namespace game

class Game

{

static player1

static player2

}

Close the code editor, right-click the node of the script in the project tree, select **Rename**, and name it **Game**.

As you can see, I want to create a class that will hold references to our two tank instances. This way, wherever we need to access them, we'll have these references ready to use.

Back to our **obj_tank** editor, add a **Create** event and add this script to it:

/// set player 1 and player 2

// first, disable sprite animation (showing another subimage each time)

imageSpeed = 0

// if this is player 2, and everything is already set up, return

if Game.player1 != null

{

// we just need to set these properties before we return

player1 = false

otherPlayer = Game.player1

imageIndex = 1

return null

}

// find the other player tank

otherPlayer = nearestInstance(x, y, &lt;obj_tank&gt;, /\* instance to ignore: \*/ this) ?? throw new Exception("Game must have 2 players.")

player1 = true

Game.player1 = this

Game.player2 = otherPlayer

// lock view 0 on player 1 and view 1 on player 2

setViewFollowingTarget(0, this)

setViewFollowingTarget(1, otherPlayer)

// set player 1 keys

k_right = Keys.d

k_left = Keys.a

k_up = Keys.w

k_down = Keys.s

k_shoot = Keys.q

k_laser = Keys.e

This code does a few things related to setting up players 1 and 2:

- Makes view 1 follow player 1 and view 2 follow player 2.
- Sets different keys for player 1.
- Sets the image index of player 2 to 1 and disables animation for both players.
- Saves references to each player in the **Game** class and, for each instance, saves a quick reference to the other instance.
- Sets the **player1** boolean value.

Now we need to modify our movement script to use our key properties when checking for input. Select the **Step** event and open the **movements** script, then replace the values in the input functions with our new **k_x** properties.

Run the game. If everything went **well**, the tanks should have different colors and control keys, and each view should follow a different player.

**Lasers and views filtering**

We want to add a laser flashlight for the tanks that can show the path the bullet is expected to travel, like in the picture:

But there's a little problem – we only want the laser to be shown in the view of its tank. We don't want to see the enemy's laser in our view, because it should be a tool that helps the player without distracting the other player. How can we make our laser appear only in the correct view?

When views are enabled, the **Draw** events are fired once for each view, so the world is drawn separately using the current view's transformation each time. The index of the view currently being drawn is passed as an argument to the **Draw** event, and we can use it to filter our drawings between the different views. This parameter is called **currentViewIndex** and is only accessible through the **Draw** event.

To draw the laser, we need to check for collisions with solid objects at each step from the tank forward, up to a specific range. If we detect a collision, we'll change the horizontal or vertical direction, just as we do for the bullets, and store the collision position in a list. At the end, we'll draw a line between all the positions in the list.

Before we write the algorithm, we need to define a class that represents a position. Right-click the **Scripts** directory in the project tree, select **Create Script**, and write the class:

namespace game

class Vector2 (x, y)

{

constructor(x, y)

{

this.x = x

this.y = y

}

}

Close the code editor, right-click the node of the script in the project tree, select **Rename**, and name it **Vector2**.

We also need to modify the **Game** class we created earlier and add a static property that will represent the speed of the bullets. We need to know their speed in order to calculate the bullet's path.

So, open the **Game** script and change the code to look like this:

namespace game

class Game ()

{

static player1

static player2

static const bulletSpeed = 9

}

In the **obj_bullet Create** event, open the **set speed** script and change the current value there to Game.bulletSpeed.

Click OK and close the **obj_bullet** editor. Then go to the **obj_tank** editor and create two properties: **laserRange** and **k_laser**, both with their **type** set to **Number**.

For the **laserRange** value, enter a range in pixels, e.g. 300. For **k_laser**, enter the key you want player 2 to use to activate their laser, for example Keys.m.

Then go to the Create event and change the laser key for player 1 in the section where we already set the other keys:

k_laser = Keys.e

Add a **Draw** event and insert the laser calculation and drawing algorithm:

/// draw laser

if keyDown(k_laser) & player1 != null

{

// compute laser path

if currentViewIndex == (player1 ? 0 : 1)

{

var hs = lengthDirX(Game.bulletSpeed, direction), vs = lengthDirY(Game.bulletSpeed, direction)

var cx = x, cy = y

const laserPath = new List

for (var d = 0; d <= laserRange; d += Game.bulletSpeed)

{

if otherPlayer.pointMeeting(cx, cy)

{

break

}

var intersect = false

if !pointFree(cx + hs, cy)

{

hs = hs \* -1

intersect = true

}

if !pointFree(cx, cy + vs)

{

vs = vs \* -1

intersect = true

}

if intersect

{

laserPath.add(new Vector2(cx, cy))

}

cx += hs

cy += vs

}

laserPath.add(new Vector2(cx, cy))

// draw the laser

setColor(player1 ? Color.red : Color.green)

var vx = x, vy = y

foreach v in laserPath

{

drawLine(vx, vy, v.x, v.y)

vx = v.x

vy = v.y

}

}

}

If you run the game now, the tanks will disappear! This is because, by default, if an object does not have a **Draw** event, the engine renders it normally. However, when an object has a **Draw** event, the engine lets that object decide if and when it should draw itself.

This allows you to control the depth of your drawings. For example, if you want to draw a filled rectangle around an object, with the object itself drawn on top of it, you should first draw the rectangle and then draw the object itself.

Our **Draw** event currently does not call drawSelf(), so the tanks are invisible.

Add a new script to the **Draw** event and put this code into it:

/// draw self

drawSelf()

Now you can run the game and check the laser.

I hope I was clear in my explanations in this part. Views can sometimes be complicated, especially when you have multiple views. The next part will be coming soon. You can see a spoiler in the GIF – there's a little HP bar above the tanks...
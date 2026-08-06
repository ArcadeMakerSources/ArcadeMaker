# Make Your First ArcadeMaker Game

In this tutorial, we'll learn how to create a tank battle game. After cloning the full source code, make sure you can successfully run the ArcadeMaker.IDE project, and you'll be ready to begin.

**Loading first sprites**

We'll start by loading the sprites for the tank and the bullets. For this game, I used the [Top-Down Tanks Redux](https://kenney-assets.itch.io/top-down-tanks-redux) sprite asset.

To create a sprite, right-click the _Sprites_ folder in the project tree and select _Create Sprite_:

<img width="252" height="325" alt="amtut create sprite" src="https://github.com/user-attachments/assets/e4219991-4855-4ebe-9626-42fc49d9337a" />

In the editor that opens, set the name to "spr_tank", then click Import and select red tank image.

The next thing we need to do is set the sprite's origin point. The origin point is the pixel that the object's x and y properties refer to. It is also the point around which the sprite rotates. In other words, the origin point is the only pixel that remains fixed when the sprite rotates.

By default, a new sprite's origin is set to (0, 0). Since we want our tank to rotate around its center, click the Center button in the Origin panel.

Your editor should now look like this:

<img width="804" height="496" alt="amtut spr_tank editor" src="https://github.com/user-attachments/assets/b0b243be-22fe-4a79-a176-add8151b8c13" />

Click **OK**, then create another sprite for the bullet. I simply used a black **4×4** rectangle as the bullet sprite. You can create one by clicking **Edit → New**, or you can import your own image.

**Creating the tank object**

So far, we've only imported the images (sprites) for the tank and the bullet. However, sprites by themselves don't do anything. To make our tank move and shoot, we need to create objects and assign those sprites to them.

Right-click the **Objects** folder in the Project Tree and select **Create Object**. Name the object **obj_tank**, and in the **Sprite** field, select **spr_tank**.

<img width="305" height="281" alt="amtut select sprite tank" src="https://github.com/user-attachments/assets/75bd3605-d6c2-4505-8a19-8a108e460fd1" />

Now it's time to write some code!

We'll begin by creating two properties: **moveSpeed** and **rotationSpeed**. As their names suggest, these properties control how fast the tank moves and rotates.

Mark both properties as **constant**, since we don't intend to change them during gameplay. For now, every tank will move and rotate at the same speed.

As you can see, each property lets you choose its value type. Select **Number**, then set both values to **5**.

<img width="396" height="189" alt="amtut obj_tank moves and rotations" src="https://github.com/user-attachments/assets/c0304726-626f-4c21-90c8-890649d34eea" />

Click **Add Event** and choose **Step**.

The **Step** event runs once every frame, making it the perfect place for our movement logic.

Click **Add Script**. A new script will appear in the list above the button. Double-click it to open the code editor.

Whenever you create a new script for an event, it's good practice to start by describing what the script does. Keeping your code documented makes large projects much easier to maintain.

You can add a description by starting the script with three forward slashes (///), followed by a short explanation. For example:

`/// movements`

Now we're ready to write the movement logic.
```
/// movements

// backward / forward
if keyDown(Keys.up)
{
  speed = moveSpeed
}
else
{
  if keyDown(Keys.down)
  {
    speed = 0 - moveSpeed
  }
}

// rotating
var rotation = 0
if keyDown(Keys.right)
{
  rotation = rotationSpeed
}
else
{
  if keyDown(Keys.left)
  {
    rotation = 0 - rotationSpeed
  }
}

// if one or morre of the keys we've just checked was pressed, make the move
if (rotation != 0 | speed != 0)
{
  imageAngle += rotation
  direction = imageAngle // updates hspeed & vspeed
  x += hspeed
  y += vspeed
  speed = 0 // otherwise the engine core would also make the move
  
  // if we're colliding with something solid, cancel the move
  if !placeFree(x, y)
  {
    x -= hspeed
    y -= vspeed
    // then (not before, bc setting "direction" would update hspeed and vspeed to their previous values)
    imageAngle -= rotation
    direction = imageAngle
  }
}
```

This code uses four special built-in properties that are part of the ArcadeMaker engine: **speed**, **direction**, **vspeed**, and **hspeed**.
Setting either **speed** or **direction** (in degrees) automatically updates **vspeed** (vertical speed) and **hspeed** (horizontal speed). Likewise, changing **vspeed** or **hspeed** updates **speed** and **direction** accordingly.
At the time of writing, the **Exp** language (the language used by the engine) has a limitation: using if, while, and similar statements without braces ({}) is not recommended. This also applies to **else if**. Instead, write it like this: `else { if (...) { ... } }`. This is caused by a known bug that will be fixed in a future version.
Click _OK_ to save the script.

**Creating a Room**
Before we can test our movement logic, we need to create a room.
A **room** is what many other engines call a **level**, **map**, or **world**. It defines the space where game objects exist.
Right-click the **Rooms** folder in the Project Tree and select **Create Room**.

<img width="245" height="284" alt="amtut create room" src="https://github.com/user-attachments/assets/fb29020e-ae0d-4886-afd3-cc2b6929ef64" />

In the Room Editor, open the **Settings** tab and set the room's name to **rm_level1**. You can also give it a caption if you'd like.
Next, switch to the **Objects** tab and select **obj_tank** from the object list.

<img width="277" height="278" alt="amtut select object to place in room" src="https://github.com/user-attachments/assets/7ba87d96-9c02-44e0-86e1-c9eb05e0b55f" />

Now you can place tanks in the room by clicking anywhere on the room grid.
Place a few tanks, then click **Run** on the top toolbar to start the game and test the movement system. Once the game starts, all of the tanks should respond to the **arrow keys**, moving forward and backward while rotating left and right.

**Shooting**
The final feature we'll add in this part is shooting.
First, create a new object called **obj_bullet**.
For now, it only needs a single **Create** event that sets its movement speed.
Click **Add Event → Create**, then add the following script:
```
/// set speed
Speed = 9
```
That's all we need for the bullet object for now.
Click **OK**, then double-click **obj_tank** in the Project Tree to reopen its editor.
Add a new event by selecting **Add Event → KeyPress → Space**.
The following script will run whenever the **Space** key is pressed:
```
/// shoot
const bullet = createInstance(x, y, <obj_bullet>)
bullet.direction = direction
```
Click **OK** to close the code editor, then run the game.
Your tank should now fire bullets whenever you press the **Space** key.
If you look closely, you'll notice that newly created bullets initially appear at the center of the tank. This happens because the bullet is created using the tank's origin point, which we previously set to the center of the sprite.
The easiest way to solve this is by setting the bullet object's **Depth** property to **1** (in the **obj_bullet** editor). This causes the bullets to be drawn behind the tanks, so while they overlap, they remain hidden.

If everything worked correctly, your game should now look like this:
<img width="640" height="510" alt="amtut part1 result" src="https://github.com/user-attachments/assets/e096b1f9-8e2d-4b57-80f7-b39a40776c8e" />

Hope this part helped you!

If by the time you read this part 2 is not there yet, then it means I'll upload it tomorrow (if it's tomorrow and it didn't happen yet, read this sentence again).

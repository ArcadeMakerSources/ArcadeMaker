# Part 2: Walls, Backgrounds and Bouncing Shots

<img width="638" height="508" alt="amtut p2 result" src="https://github.com/user-attachments/assets/046c4da1-09da-41b3-99f8-75f1629c104e" />


Let's add some soul to our game: we'll start with the walls.

**Adding the Walls**
Create a new **spr_wall** sprite and load the **createMetal** sprite from our sprite asset.
Create an **obj_wall** object, set its sprite to the one you just created, and check the **Solid** checkbox.

<img width="269" height="223" alt="amtut solid checkbox" src="https://github.com/user-attachments/assets/1ccee1fe-cf1e-455a-b61e-f634de820c2b" />

Functions like `placeFree(x, y, [angle])` only test objects with this property set to **true**.
The script we put in the **Step** event of **obj_tank** in Part 1 is supposed to prevent the tank from moving into solid objects.
Double-click **rm_level1** in the project tree to open the room editor, and place some walls near the tank.
Run the game. The tank should now stop when trying to collide with the walls.

<img width="877" height="596" alt="amtut room preview" src="https://github.com/user-attachments/assets/59c47473-7c16-4353-99fe-6b95d56fc23c" />

**Backgrounds**
I used this texture for the background:

<img width="32" height="32" alt="sand" src="https://github.com/user-attachments/assets/57f65cbd-d51f-44de-88c6-b2b651bc7719" />

To create a background, right-click the **Backgrounds** folder in the project tree and select **Create Background**. Load this or your own background texture, call it **bg_sand**, and click **OK**.
Go back to the room editor and select the **Backgrounds** tab. Select the first layer in the list view (**Background 0** or whichever number the list begins from) and make sure that the **Visible when room starts** checkbox is checked.
Next, click the box that shows **&lt;No Background&gt;** and select **bg_sand**. The room preview should now use this background, and if you run the game, you'll see it there as well.

<img width="197" height="508" alt="amtut select background" src="https://github.com/user-attachments/assets/d59c2c07-6cd3-4385-a2db-860290e36da1" />

**Bullet Bouncing**
We want the bullet to bounce when it hits a solid object like a wall. We've already talked about the `hspeed` and `vspeed` properties of game objects; multiplying them by -1 when hitting a wall in their direction would do the job.

Double-click **obj_bullet**, add a **Step** event, and insert this script into it:
```
/// bounce on collision with solid objects
if !placeFree(x + hspeed, y)
{
    hspeed = hspeed \* -1
}

if !placeFree(x, y + vspeed)
{
    vspeed = vspeed \* -1
}
```
We also want to make sure the bullets cannot keep bouncing for too long. We would want to set a timer to destroy them. Add this script to the **Create** event:
```
/// init self-destruction alarm
setAlarm(0, 500)
```
This sets Alarm 0 to 500 frames. After 500 frames, the Alarm 0 event will fire. Add this event to the events list and add this script to it to destroy the bullet:
```
/// destroy self 
destroy()
```

<img width="669" height="164" alt="amtut obj_bullet destroy self" src="https://github.com/user-attachments/assets/f453facf-5fe6-4a05-873e-a19137aaca54" />

**Shot Sound**
We want to have a sound effect played when a bullet is shot. Create a sound and name it **snd_shot**, and load the shot sound from the sounds directory.
Notice: As of writing this tutorial, the **Play** button in the sound editor doesn't work. Anyway, make sure to select **Sound Effect** in the **Kind** selection.
Sound effects should be short and can be played simultaneously, like bonus collection, jumping, and, of course, shooting. Use **.wav** files for them.
Background music can only be played once at a time, but it can be long. Use **.mp3** files for it.

<img width="194" height="410" alt="amtut sound editor" src="https://github.com/user-attachments/assets/c0c45225-d689-4396-a38f-c859c8bf57f9" />

Click **OK** and double-click **obj_bullet**. Select the **KeyPress** event where we've inserted the shooting script, open the script, and add the following line:
```
playSound(Sounds.snd_shot)
```
Run the game, and the shot sound should play every time you shoot.

That's it! I hope this was useful. I hope I'll be able to make Part 3 next week.

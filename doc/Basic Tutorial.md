## 1. Sprites
Sprites can contain a single image or a sequence of images that form an animation. It’s recommended to import images from files rather than drawing them from scratch, because the built‑in image editor is not even 50% complete. You can use it for very simple shapes and drawings, though.

By default, the origin point for positioning and rotation is (0, 0) relative to the object’s position. You can change the origin by clicking the image preview in the sprite editor or by manually editing the values in the text fields. You can also set a custom collision mask for your sprite in the sprite editor.

## 2. Objects
You can assign a sprite to an object using the selection box on the right. Control the animation speed by setting the imageSpeed property in the Create event (imageSpeed = number of game frames before increasing imageIndex → higher value = slower animation).

To add events, click Add Event and choose Create, Step, Draw, KeyPress or any other event.

To add a script to execute when the event is fired, select the event from the list, click Add Script, then double‑click the new item that appears. Start your scripts with `/// description`.

If you add code to the Draw event, then only what you draw manually will appear. The sprite will not render unless you call drawSelf().

## 3. Sounds
There are two types of sounds: background music and sound effects. Multiple sound effects can play simultaneously, but only one background music track can play at a time. Use .mp3 for background music and .wav for sound effects.

To play a sound:
`
playSound(Sounds.soundName, /* repeating: */ false)
`

Note: the “Play” button in the sound editor is currently non‑functional.

## 4. Paths
You can use the path editor to create paths that objects can follow or that can be drawn using built‑in functions:

```
startPath(Paths.pathName, /* speed: */ 4, /* endAction: */ PathEndAction.reverse, /* absolute: */ true)

drawPath(path, x, y, width)
```

## 5. Rooms
Use the room editor to design levels. Select an object and place it in the room by clicking on the map. You can set the room size and window caption in the Settings tab. You can also add views (cameras) and make them follow your player.

## 6. Backgrounds
To add a background to the room, first load it by clicking the background icon in the top menu strip, then in the room editor select "Backgrounds" tab and select your background. You can make it scrolling by setting its speed values.

## 7. Fonts
Font support is not fully implemented, but you can already create a font, choose its family, italic style, and size, and then use `drawText(x, y, text)` in the Draw event.

To align text with a view, use:
- getViewX(viewIndex)
- getViewY(viewIndex)
- getViewWidth(viewIndex)
- getViewHeight(viewIndex)

You can also use the currentViewIndex argument (passed to the Draw event) to draw text only for a specific view.

## 8. Collision Detection
The only collision method currently available is rotated rectangles. The engine automatically generates a minimal rectangle around the non‑transparent pixels of a sprite (the collision mask, editable in the sprite editor). It uses the object’s x, y, origin point and imageAngle to check collisions.

Available functions:
- placeMeeting(x, y, typeOrInst)
- instanceMeeting(x, y, type)
- placeFree(x, y) — checks collision with any object marked as solid
- placeEmpty() — not implemented yet

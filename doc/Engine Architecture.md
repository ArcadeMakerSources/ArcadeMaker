Engine Architecture Documentation

Overview
This engine is built around a strict separation of responsibilities between three components:

- Backend Engine (MonoGame) — owns the main loop, timing, delta‑time, initialization, content loading, and disposal.  
- GameRunner — dispatches backend events into the engine core and manages user‑requested room switching.  
- IGame Interface — defines the engine API surface and exposes backend‑implemented rendering and audio functions.

The engine uses rooms as its fundamental unit of game state.  
This architecture ensures the engine core remains backend‑agnostic, portable, and easy to extend.

---

1. Core Components

1.1 Backend Engine
The backend engine (e.g., MonoGame) is the driver of the entire system.

Backend Responsibilities
- Owns the main loop  
- Calculates delta‑time  
- Calls update and draw events  
- Implements initialization (Initialize())  
- Implements content loading (LoadContent())  
- Implements disposal (Dispose())  
- Manages the graphics device  
- Implements rendering and audio  
- Handles windowing, input, and platform services  
- Calls GameRunner.FireStep() and GameRunner.FireDraw()  

The backend is the only component that interacts directly with the platform.

---

1.2 GameRunner
GameRunner is the engine’s event dispatcher and room manager.  
It does not own timing, initialization, content loading, disposal, or room updates.

GameRunner Responsibilities
- Holds a reference to the active IGame implementation  
- Receives update and draw events from the backend  
- Dispatches those events into the engine core  
- Iterates over renderable instances  
- Calls backend‑implemented render API functions  
- Provides user‑callable room switching (GoToRoom(...))  

GameRunner Does NOT   
- Update rooms automatically  
- Control update frequency    
- Perform initialization  
- Load content  
- Manage disposal  
- Perform GPU rendering  

---

1.3 IGame Interface
IGame defines the engine-facing API used by both the backend and the engine core.

IGame Contains
- Rendering API functions (implemented by backend)  
- Audio API functions (implemented by backend)  
- Input helpers  
- Asset management helpers  
- Room helpers  
- Engine utility functions  

IGame Does NOT
- Contain Step()  
- Contain Draw()  
- Own the lifecycle  
- Manage timing   
- Perform initialization  
- Load content  
- Handle disposal  

---

2. Engine Lifecycle

2.1 Startup Sequence
All initialization is performed by the backend.

1. Backend initializes platform systems.  
2. Backend creates GameRunner.  
3. Backend implements an IGame instance.  
4. Backend calls its own:
   - Initialize()
   - LoadContent()

GameRunner is not involved in initialization.

---

2.2 Main Loop
The backend controls the main loop and timing.

Update Phase
Backend calls:

`
GameRunner.FireStep()
`

Draw Phase
Backend calls:

`
GameRunner.FireDraw()
`

GameRunner simply forwards these events into the engine.

---

2.3 Disposal
Disposal is backend‑owned.

1. Backend triggers disposal  
2. Backend calls its own Dispose()  
3. Backend cleans up platform resources  

GameRunner does not participate in disposal.

---

3. Update Pipeline

3.1 FireStep()
FireStep() is the only update entry point in the engine.

It is called once per update tick by the backend.

FireStep Responsibilities
- Forward the update event into the engine core  

---

4. Rendering Pipeline

Rendering is a cooperative process between:

- The backend engine  
- GameRunner  
- The backend‑implemented IGame render API  

---

4.1 Rendering Flow

Step 1 — Backend render tick
Backend calls:

`
GameRunner.FireDraw()
`

Step 2 — GameRunner prepares the draw
FireDraw():

- Collects renderable instances from active rooms  
- Sorts them if needed  
- Iterates through them  

Step 3 — GameRunner calls backend render API
For each instance (unless it overrides Draw event):

`
IGame.DrawInstance(instance)
`

For engine‑level drawing:

`
IGame.DrawLine(...)
IGame.DrawRect(...)
IGame.DrawText(...)
`

Step 4 — Backend performs actual GPU drawing
The backend uses its graphics API to draw.

Step 5 — Backend finalizes the frame
Backend presents the frame.

---

5. Rendering API (IGame)

These functions are declared in IGame but implemented by the backend.

DrawInstance(Runtime.Instance instance)
Draws a renderable instance.

DrawLine(...)
Draws a line.

Additional Rendering Functions
Backends may implement:

- DrawTexture(...)  
- DrawText(...)  
- DrawRectangle(...)  
- DrawMesh(...)  
- DrawCircle(...)  

The engine core never touches the graphics device directly.

---

6. Backend Responsibilities Summary

The backend engine must:

- Own the main loop  
- Call GameRunner.FireStep()  
- Call GameRunner.FireDraw()  
- Implement all IGame rendering functions  
- Implement all IGame audio functions  
- Manage GPU resources  
- Handle windowing and input  
- Perform initialization  
- Load content  
- Control disposal  

---

7. Architecture Diagram

`
+---------------------------+
|       Backend Engine      |
|  (MonoGame, SDL, etc.)    |
|---------------------------|
| - Main loop               |
| - Delta-time              |
| - Initialize/LoadContent  |
| - GPU / Audio             |
| - Windowing / Input       |
| - Calls FireStep/FireDraw |
| - Dispose()               |
+-------------+-------------+
              |
              v
+---------------------------+
|        GameRunner         |
|---------------------------|
| - Dispatch Step events    |
| - Dispatch Draw events    |
| - GoToRoom(...)           |
| - Collect instances       |
| - Call IGame render API   |
+-------------+-------------+
              |
              v
+---------------------------+
|          IGame            |
|---------------------------|
| - Render API (backend)    |
| - Audio API (backend)     |
| - Room helpers            |
| - Asset helpers           |
| - Engine utilities        |
+---------------------------+
`

---

8. Summary

The engine architecture is built on a clean separation:

- Backend controls timing, initialization, content loading, rendering, audio, and disposal.  
- GameRunner dispatches backend events and handles user‑requested room switching.  
- IGame defines the engine API and exposes backend‑implemented rendering functions.

This design makes the engine backend‑agnostic, modular, portable, and easy to extend.

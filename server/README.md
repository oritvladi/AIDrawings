# Server

This is an ASP.NET Core Web API server for managing interactive drawings and shapes, with AI integration (Gemini) for prompt-based drawing creation.

## Features

- **Prompt-based Drawing:**  
  Users can send a free-text prompt and receive a new shape generated by the Gemini AI, based on supported shapes and the current canvas state.

- **Shape Management:**  
  Each drawing consists of multiple shapes (circle, rectangle, line, triangle, etc.), each with precise position and size on the canvas.

- **Drawing Operations:**  
  - Add a shape to a drawing (with AI assistance)
  - Remove the last shape (Undo)
  - Save a drawing
  - Delete a drawing
  - Get a drawing by ID

- **Shared Shape Model:**  
  The shape model is designed to be shared between client and server, ensuring consistency.

## Project Structure
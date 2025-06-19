# 🎨 AI Drawing Bot

**AI Drawing Bot** is a smart drawing application powered by artificial intelligence.

Just enter a **prompt** like _"draw a house with a triangular roof"_, and the bot will decompose the idea into basic shapes and draw them on an interactive canvas.

🧠 Key Features:
- Undo the last drawing
- Save and revisit past drawings
- Real-time drawing on a visual canvas

---

## 🚀 How to Run the Project

### 🖥️ Run the Server (C# / ASP.NET Core)

```bash
cd server
dotnet run
The server will run at http://localhost:5000

💻 Run the Client (React)
bash
Copy
Edit
cd client
npm install
npm start
The client will open at http://localhost:3000 and connect to the server

⚙️ Environment Variables
This project uses .env files (not included in the repository).
Please create them manually under the server and client folders if needed.

📁 Project Structure
pgsql
Copy
Edit
AI-Drawing-Bot/
├── server/      ← ASP.NET Core server code
├── client/      ← React front-end code
└── README.md    ← This file
Enjoy drawing with AI! 🎨🤖

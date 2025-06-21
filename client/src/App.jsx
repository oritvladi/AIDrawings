import React, { useState, useEffect } from "react";
import "./App.css";

function renderShape(shape, i) {
  console.log(shape);

  const { type, x, y, width, height, color } = shape;

  switch (type) {
    case "Rectangle":
      return (
        <rect
          key={i}
          x={x}
          y={y}
          width={width}
          height={height}
          fill={color}
          stroke={color}
        />
      );

    case "Square":
      return (
        <rect
          key={i}
          x={x}
          y={y}
          width={width}
          height={width}
          fill={color}
          stroke={color}
        />
      );

    case "Circle":
      return (
        <ellipse
          key={i}
          cx={x}
          cy={y}
          rx={width / 2}
          ry={width / 2}
          fill={color}
          stroke={color}
        />
      );

    case "Ellipse":
      return (
        <ellipse
          key={i}
          cx={x}
          cy={y}
          rx={width / 2}
          ry={height / 2}
          fill={color}
          stroke={color}
        />
      );

    case "Line":
      return (
        <line
          key={i}
          x1={x}
          y1={y}
          x2={x + width}
          y2={y + height}
          stroke={color}
          strokeWidth="3"
        />
      );

    case "Triangle":
      return (
        <polygon
          key={i}
          points={`
            ${x + width / 2},${y}              
            ${x},${y + height}                  
            ${x + width},${y + height}         
          `}
          fill={color}
          stroke={color}
        />
      );

    default:
      return null;
  }
}

export default function App() {
  const [canvases, setCanvases] = useState([
    { id: null, name: "new canvas", drawings: [], isLoaded: true }
  ]);
  const [selectedCanvasIdx, setSelectedCanvasIdx] = useState(0);
  const currentDrawings = canvases[selectedCanvasIdx]?.drawings || [];
  const [undoStack, setUndoStack] = useState([]);
  const [redoStack, setRedoStack] = useState([]);
  const [prompt, setPrompt] = useState("");
  const [loading, setLoading] = useState(false);
  const [messages, setMessages] = useState([]);
  const [showSaveDialog, setShowSaveDialog] = useState(false);
  const [canvasName, setCanvasName] = useState("");
  const [watchCanvases, setWatchCanvases] = useState(false);

  useEffect(() => {
    const canvas = canvases[selectedCanvasIdx];
    if (!canvas) return;
    if (canvas.id == null) {
      setWatchCanvases(false);
      return;
    }
    setWatchCanvases(true);
    if (!canvas.isLoaded) {
      setLoading(true);
      (async () => {
        try {
          const res = await fetch(`/api/drawings/${canvas.id}`);
          if (!res.ok) {
            alert("Error loading the canvas");
            return;
          }
          const data = await res.json();
          setCanvases(prev => prev.map((c, idx) => {
            if (idx !== selectedCanvasIdx) return c;
            return {
              ...c,
              drawings: data.drawings,
              isLoaded: true,
            };
          }));
          console.log("Loaded canvas");
        } catch {
          alert("Communication error with the server");
        } finally {
          setLoading(false);
        }
      })();
    }
  }, [selectedCanvasIdx, canvases]);

  useEffect(() => {
    const loadCanvases = async () => {
      try {
        const res = await fetch("/api/drawings/all-canvases");
        if (res.ok) {
          const data = await res.json();
          const loadedCanvases = data.map(canvas => ({
            id: canvas.id,
            name: canvas.title,
            drawings: [],
            isLoaded: false,
          }));
          setCanvases(prev => [{ id: null, name: "new canvas", drawings: [], isLoaded: true }, ...loadedCanvases]);
        } else {
          alert("Error loading the canvases");
        }
      } catch (err) {
        alert("Communication error with the server");
      }
    };
    loadCanvases();
  }, []);

  function handleClearDrawing() {
    setUndoStack(prev => [...prev, currentDrawings]);
    setRedoStack([]);
    setCanvases(prev => {
      const next = [...prev];
      next[selectedCanvasIdx] = { ...next[selectedCanvasIdx], drawings: [] };
      return next;
    });
  }

  function handleUndo() {
    if (undoStack.length === 0) return;
    const stateToRestore = undoStack[undoStack.length - 1];
    setRedoStack(prev => [currentDrawings, ...prev]);
    setUndoStack(prev => prev.slice(0, -1));
    setCanvases(prev => {
      const next = [...prev];
      next[selectedCanvasIdx] = { ...next[selectedCanvasIdx], drawings: stateToRestore };
      return next;
    });
  }

  function handleRedo() {
    if (redoStack.length === 0) return;
    const stateToRestore = redoStack[0];
    setUndoStack(prev => [...prev, currentDrawings]);
    setRedoStack(prev => prev.slice(1));
    setCanvases(prev => {
      const next = [...prev];
      next[selectedCanvasIdx] = { ...next[selectedCanvasIdx], drawings: stateToRestore };
      return next;
    });
  }

  function handleSave() {
    setCanvasName(canvases[selectedCanvasIdx]?.name === "new canvas" ? "" : canvases[selectedCanvasIdx]?.name);
    setShowSaveDialog(true);
  }

  async function handleSaveCanvas() {
    if (!canvasName.trim()) return;
    setShowSaveDialog(false);
    try {
      const response = await fetch("/api/drawings/save-canvas", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          title: canvasName,
          drawings: currentDrawings
        })
      });

      if (response.ok) {
        const saved = await response.json();
        setCanvases(prev => {
          const newSavedCanvas = {
            id: saved.id,
            name: saved.name,
            drawings: currentDrawings,
            isLoaded: true,
          };

          let updatedCanvases = [...prev];
          const mainCanvasIdx = prev.findIndex(c => c.id === canvases[selectedCanvasIdx].id);

          if (mainCanvasIdx !== -1) {
            updatedCanvases[mainCanvasIdx] = newSavedCanvas;
          } else {
            updatedCanvases.push(newSavedCanvas);
          }

          const newEmptyCanvas = { id: null, name: "new canvas", drawings: [], isLoaded: true };
          updatedCanvases.push(newEmptyCanvas);
          setSelectedCanvasIdx(updatedCanvases.length - 1);
          return updatedCanvases;

        });
        setMessages([]);
        setRedoStack([]);
        setUndoStack([]);
        alert("Canvas saved successfully! Moving to a new canvas.");
      } else {
        alert("Error saving the canvas");
      }
    } catch (err) {
      alert("Communication error with the server");
    }
  }

  async function handleSelectCanvas(idx) {
    setSelectedCanvasIdx(idx);
    setUndoStack([]);
    setRedoStack([]);
    setMessages([]);
  }

  async function handleSend() {
    if (!prompt.trim()) return;
    setMessages([...messages, { from: "user", text: prompt }]);
    setLoading(true);

    try {
      const response = await fetch("/api/drawings/add-draw", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          prompt,
          existingDrawings: currentDrawings
        })
      });

      if (!response.ok) throw new Error("Server error");

      const newShapes = await response.json();

      setUndoStack(prev => [...prev, currentDrawings]);
      setRedoStack([]);

      setCanvases(prev => {
        const next = [...prev];
        const newDrawing = { description: prompt, shapes: newShapes };
        const currentCanvas = next[selectedCanvasIdx];
        next[selectedCanvasIdx] = {
          ...currentCanvas,
          drawings: [...currentCanvas.drawings, newDrawing],
        };
        return next;
      });

      setMessages(msgs => [...msgs, { from: "bot", text: "Drawing added successfully!" }]);
      setPrompt("");
    } catch (err) {
      setMessages(msgs => [...msgs, { from: "bot", text: "Error communicating with the server" }]);
    }
    setLoading(false);
  }

  return (
    <div className="app-container">
      <div className="toolbar">
        <select className="canvas-select" value={selectedCanvasIdx} onChange={e => handleSelectCanvas(Number(e.target.value))}>
          {canvases.map((canvas, idx) => (
            <option key={canvas.id ?? `new-${idx}`} value={idx}>{canvas.name}</option>
          ))}
        </select>
        <button
          className="toolbar-btn orange"
          onClick={handleUndo}
          disabled={undoStack.length === 0 || watchCanvases}
        >
          undo
        </button>
        <button
          className="toolbar-btn purple"
          onClick={handleRedo}
          disabled={redoStack.length === 0 || watchCanvases}
        >
          redo
        </button>
        <button
          className="toolbar-btn off-white"
          onClick={handleClearDrawing}
          disabled={watchCanvases}
        >
          clear
        </button>
        <button
          className="toolbar-btn yellow"
          onClick={handleSave}
          disabled={watchCanvases}
        >
          save
        </button>

      </div>
      <div className="main-content">
        <div className="chat-section">
          <div className="chat-title">chat history</div>
          <div className="chat-messages">
            {messages.map((msg, i) => (
              <div key={i} className={`chat-msg ${msg.from}`}>
                <b>{msg.from === "user" ? "you:" : "bot:"}</b> {msg.text}
              </div>
            ))}
          </div>
          <div className="chat-input-row">
            <input
              type="text"
              value={prompt}
              onChange={e => setPrompt(e.target.value)}
              placeholder="write a message..."
              disabled={loading || watchCanvases}
              onKeyDown={e => e.key === "Enter" && handleSend()}
            />
            <button onClick={handleSend} disabled={loading || !prompt.trim()}>
              send
            </button>
          </div>
        </div>
        <div className="canvas-section">
          <svg width="600" height="400">
            {currentDrawings.flatMap((drawing, dIdx) => drawing.shapes.map((shape, i) => renderShape(shape, `${dIdx}-${i}`)))}
          </svg>
        </div>
      </div>

      {showSaveDialog && (
        <div style={{ position: "fixed", top: 0, left: 0, width: "100vw", height: "100vh", background: "rgba(0,0,0,0.3)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000 }}>
          <div style={{ background: "#fff", padding: 30, borderRadius: 10, boxShadow: "0 2px 12px #0002", minWidth: 300 }}>
            <div style={{ marginBottom: 10, fontWeight: "bold" }}>canvas name:</div>
            <input
              type="text"
              value={canvasName}
              onChange={e => setCanvasName(e.target.value)}
              placeholder="insert canvas name"
              style={{ width: "100%", padding: 8, borderRadius: 6, border: "1px solid #ccc", marginBottom: 16 }}
              autoFocus
            />
            <div style={{ display: "flex", gap: 10, justifyContent: "center" }}>
              <button className="toolbar-btn green" onClick={handleSaveCanvas} disabled={!canvasName.trim()}>save</button>
              <button className="toolbar-btn orange" onClick={() => setShowSaveDialog(false)}>cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

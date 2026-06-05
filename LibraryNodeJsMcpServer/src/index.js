import dotenv from "dotenv";
import express from "express";
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { SSEServerTransport } from "@modelcontextprotocol/sdk/server/sse.js";
import { registerLibraryTools } from "./tools/libraryTools.js";

dotenv.config();

const libraryApiUrl =
  process.argv
    .find(arg => arg.startsWith("--library-url="))
    ?.split("=")[1] ||
  process.env.LIBRARY_API_URL ||
  "http://localhost:5000";

console.log(`Starting Library MCP Server with API URL: ${libraryApiUrl}...`);

function createServer() {
  const server = new Server(
    { name: "library-mcp-server", version: "1.0.0" },
    { capabilities: { tools: {} } }
  );
  registerLibraryTools(server, libraryApiUrl);
  return server;
}

if (process.env.PORT) {
  // ── HTTP SSE mode — used when deployed to Render ─────────────────────────
  const app = express();
  app.use(express.json());

  const transports = new Map();

  app.get("/sse", async (req, res) => {
    const transport = new SSEServerTransport("/message", res);
    transports.set(transport.sessionId, transport);
    res.on("close", () => transports.delete(transport.sessionId));
    const server = createServer();
    await server.connect(transport);
  });

  app.post("/message", async (req, res) => {
    const sessionId = req.query.sessionId;
    const transport = transports.get(sessionId);
    if (!transport) {
      res.status(404).json({ error: "Session not found" });
      return;
    }
    await transport.handlePostMessage(req, res, req.body);
  });

  app.listen(process.env.PORT, () =>
    console.log(`Library MCP Server listening on port ${process.env.PORT}`)
  );
} else {
  // ── Stdio mode — used locally via Program.cs ──────────────────────────────
  const server = createServer();
  const transport = new StdioServerTransport();
  await server.connect(transport);
}
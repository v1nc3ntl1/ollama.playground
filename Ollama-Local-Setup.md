# Ollama Local Setup Guide (Windows)

This guide shows how to:
1. Install Ollama on a local Windows PC.
2. Pull and run the model qwen2.5:7b.
3. Install a VS Code Ollama chat extension and prompt from the chat window.

## 1) Install Ollama on Windows

### Option A: Winget (recommended)

Open PowerShell as Administrator and run:

~~~powershell
winget install Ollama.Ollama
~~~

### Option B: Manual installer

1. Go to https://ollama.com/download/windows
2. Download and run the installer.
3. Keep default settings unless you have a specific reason to change them.

## 2) Verify Ollama is running

Open a new PowerShell window and run:

~~~powershell
ollama --version
ollama list
~~~

If Ollama is not running yet, start it:

~~~powershell
ollama serve
~~~

Note:
- If Ollama is installed with the default Windows app, it usually runs in the background automatically.
- Keep ollama serve running only if auto-start is not working on your machine.

## 3) Pull and run qwen2.5:7b

### Pull the model

~~~powershell
ollama pull qwen2.5:7b
~~~

### Quick terminal chat test

~~~powershell
ollama run qwen2.5:7b
~~~

Example prompt:

~~~text
Summarize what this model can do in 5 bullet points.
~~~

Type Ctrl+C to exit the interactive terminal chat.

## 4) Install VS Code Ollama chat extension

This guide uses extension ID: selfagency.opilot

### Install from VS Code UI

1. Open VS Code.
2. Go to Extensions (Ctrl+Shift+X).
3. Search for: Opilot
4. Install extension: selfagency.opilot

### Install from command line (optional)

~~~powershell
code --install-extension selfagency.opilot
~~~

## 5) Configure extension to use local Ollama

In the extension settings:

1. Set Ollama base URL to:

~~~text
http://localhost:11434
~~~

2. Set default model to:

~~~text
qwen2.5:7b
~~~

3. Open the extension chat panel/window and start prompting.

Example prompt to test:

~~~text
You are my coding assistant. Explain the purpose of this project in simple terms.
~~~

## 6) Troubleshooting

### Cannot connect to Ollama

Check local API endpoint:

~~~powershell
Invoke-RestMethod http://localhost:11434/api/tags
~~~

If this fails:
1. Start Ollama: ollama serve
2. Confirm no firewall or proxy is blocking localhost:11434

### Model not found in extension

Run:

~~~powershell
ollama list
~~~

If qwen2.5:7b is missing, pull it again:

~~~powershell
ollama pull qwen2.5:7b
~~~

### VS Code command not found (code)

In VS Code, open Command Palette and run:
- Shell Command: Install 'code' command in PATH

Then reopen terminal and retry the install command.

## 7) Daily use quick commands

~~~powershell
ollama list
ollama run qwen2.5:7b
~~~

You now have:
- Local Ollama runtime
- qwen2.5:7b model available
- VS Code chat workflow using local model

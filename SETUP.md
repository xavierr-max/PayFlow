# PayFlow - Setup & Running Guide

## Project Structure

- **PayFlow.API** - Backend REST API (.NET 10.0)
- **PayFlow.Domain** - Shared domain entities and models
- **Payflowfigma** - Frontend React + TypeScript (Vite)

## Prerequisites

- **.NET 10.0 SDK** - For backend development
- **Node.js 18+** - For frontend development
- **pnpm** - Package manager for frontend (installed globally)

## Installation

### 1. Install Backend Dependencies

```bash
cd PayFlow
dotnet restore
```

### 2. Install Frontend Dependencies

```bash
cd Payflowfigma
pnpm install
```

## Running the Project

### Backend (API Server)

```bash
cd PayFlow
dotnet run --project PayFlow.API/PayFlow.API.csproj
```

The API will start at `http://localhost:5000`

**Health Check:** Visit `http://localhost:5000/api/health` to verify the API is running.

### Frontend (Development Server)

In a new terminal:

```bash
cd Payflowfigma
pnpm run dev
```

The frontend will start at `http://localhost:5173` (or `http://localhost:3000` depending on Vite configuration)

## API Integration

- **Frontend API URL:** `http://localhost:5000/api`
- **CORS:** Enabled for `http://localhost:3000` and `http://localhost:5173`
- **Proxy:** Vite dev server proxies `/api/*` requests to the backend

### Environment Variables (Frontend)

Located in `Payflowfigma/.env.local`:

```
VITE_API_URL=http://localhost:5000/api
VITE_API_BASE_URL=http://localhost:5000
```

## Available API Endpoints

- `GET /` - Hello message
- `GET /api/health` - Health check endpoint

## Development Workflow

1. Start the backend: `dotnet run --project PayFlow.API/PayFlow.API.csproj`
2. Start the frontend: `cd Payflowfigma && pnpm run dev`
3. Access the app at `http://localhost:5173` (or configured port)
4. The frontend can now make requests to the API

## Building for Production

### Frontend
```bash
cd Payflowfigma
pnpm run build
```

Output will be in `Payflowfigma/dist/`

### Backend
```bash
cd PayFlow
dotnet publish -c Release
```

Output will be in `PayFlow.API/bin/Release/net10.0/publish/`

## Troubleshooting

- **CORS Error:** Ensure both frontend and backend are running, and that the CORS policy includes your frontend URL
- **API Not Responding:** Check that the backend is running on port 5000
- **Frontend Build Errors:** Run `pnpm install` again and clear `node_modules` if issues persist

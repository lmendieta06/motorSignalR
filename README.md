# 🚗 TRA-EV Motor Control System 2030

## 📋 Project Description

Intelligent electric motor control system developed for **TestResults Automotors Inc.** This project implements a complete platform for communication between vehicle computers and the TRA-EV 2030 motor, providing real-time control and advanced telemetry monitoring.

### 🎯 Main Features

- **Full REST API** for motor control with specialized endpoints
- **Real-time communication** via SignalR for telemetry
- **Interactive React dashboard** for monitoring and control
- **Realistic simulation** of motor behavior
- **Clean architecture** following SOLID principles and Clean Architecture
- **Comprehensive test suite** with unit and integration coverage
- **Safety system** with emergency stop and cooldown

---

## ⚡ Quick Start

### Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** and **npm** - [Download here](https://nodejs.org/)
- **Git** - [Download here](https://git-scm.com/)

### 🚀 Installation & Run

#### 1. Clone the repository
```bash
git clone https://github.com/lmendieta06/motorSignalR.git
cd motorSignalR
```

#### 2. Configure environment variables

**Backend (.env in the project root):**
```env
# Backend Environment Variables
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5000

# CORS Configuration
CORS_ALLOWED_ORIGINS=http://localhost:3000

# SignalR Configuration
SIGNALR_HUB_PATH=/motorhub

# Motor Simulation Configuration
TELEMETRY_BROADCAST_INTERVAL_MS=2500
MOTOR_SIMULATION_INTERVAL_MS=100

# Logging Configuration
LOGGING_LEVEL=Information
```

**Frontend (.env in frontend folder):**
```env
# Frontend Environment Variables
REACT_APP_API_BASE_URL=http://localhost:5000/api
REACT_APP_SIGNALR_HUB_URL=http://localhost:5000/motorhub
REACT_APP_ENVIRONMENT=development
REACT_APP_API_TIMEOUT=10000
REACT_APP_SIGNALR_AUTO_RECONNECT=true
REACT_APP_SIGNALR_LOG_LEVEL=Information
```

#### 3. Run the Backend

```bash
# Go to backend folder
cd backend

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the API
dotnet run --project src/MotorControl.API
```

The API will be available at: `http://localhost:5000`  
Swagger UI available at: `http://localhost:5000/swagger`

#### 4. Run the Frontend

**In a new terminal:**

```bash
# Go to frontend folder
cd frontend

# Install dependencies
npm install

# Start development server
npm start
```

The dashboard will be available at: `http://localhost:3000`

#### 5. Verify Installation

**Test the API:**
```bash
# Get motor status
curl http://localhost:5000/api/motor/status

# Set speed
curl -X POST http://localhost:5000/api/motor/speed \
  -H "Content-Type: application/json" \
  -d '{"speed": 50}'
```

**Test SignalR:**
- Open the dashboard at `http://localhost:3000`
- Verify that telemetry data updates automatically

---

## 🧪 Running Tests

### Backend Tests

```bash
# From the backend folder
cd backend

# Run all tests
dotnet test

# Specific project tests
dotnet test tests/MotorControl.Domain.Tests/
dotnet test tests/MotorControl.Application.Tests/
dotnet test tests/MotorControl.API.Tests/
```

## 🔧 Development Commands

### Backend

```bash
# Clean build
dotnet clean

# Build full solution
dotnet build
```

### Frontend

```bash
# Development with hot reload
npm start

# Build for production
npm run build
```

---

## 🌐 Development URLs

| Service             | URL                        | Description                      |
|---------------------|---------------------------|----------------------------------|
| **API Backend**     | `http://localhost:5000`   | Main API server                  |
| **Swagger UI**      | `http://localhost:5000/swagger` | Interactive API documentation    |
| **Frontend Dashboard** | `http://localhost:3000` | Main user interface              |

---

## 🏗️ Project Structure

```
motor-control-system/
├── backend/
│   ├── src/
│   │   ├── MotorControl.Domain/           # Entities and business rules
│   │   ├── MotorControl.Application/      # Services and DTOs
│   │   ├── MotorControl.Infrastructure/   # Repositories and external services
│   │   └── MotorControl.API/              # Controllers and configuration
│   ├── tests/
│   │   ├── MotorControl.Domain.Tests/
│   │   ├── MotorControl.Application.Tests/
│   │   └── MotorControl.API.Tests/
│   └── MotorControl.sln
├── frontend/
│   ├── src/
│   │   ├── components/                   # React components
│   │   ├── hooks/                        # Custom hooks
│   │   ├── services/                     # API and SignalR services
│   │   └── types/                        # TypeScript definitions
│   ├── public/
│   └── package.json
├── .env                                  # Backend environment variables
└── README.md
```
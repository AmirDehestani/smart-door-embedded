# Smart Door Embedded Simulation (F#)

## Overview
This project simulates a smart door controller using F# and .NET. 
It is designed to demonstrate concepts in functional programming, reactive event handling, and multi-threading for embedded systems.

The controller handles:
- Access requests (card swipes)
- Door lock/unlock states
- Event logging
- TCP communication with a [client app](https://github.com/AmirDehestani/smart-door-client)

## Features
- Reactive, event-driven architecture using F# MailboxProcessor
- Functional state management for door control
- Simulated multiple concurrent users
- TCP server for client communication
- Unit tests for event handling and state transitions

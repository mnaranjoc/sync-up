# FileSyncEngine

**SyncEngine** is a cross-platform file synchronization system that keeps folders in sync across multiple machines using a central coordination server.

It is built as a learning project for distributed systems concepts with .NET, focusing on real-time file synchronization between Linux and Windows machines.

## Overview

SyncEngine monitors local folders for changes and synchronizes them through a central server. When files are created, modified, or deleted, changes are propagated to other connected devices to keep all nodes consistent.

## Architecture

### Sync Agent

A background service running on each machine.

**Responsibilities:**
- Watches local file system changes
- Computes file hashes
- Sends updates to the server
- Applies remote changes

**Runs as:**
- Windows Service
- systemd service on Linux

### Sync Server

An ASP.NET Core service that coordinates synchronization.

**Responsibilities:**
- Manages connected devices
- Receives file changes
- Handles sync coordination
- Serves file data

### Storage

Uses SQLite as the primary persistence layer.

- Stores file metadata
- Tracks versions and sync state
- Maintains device registry
- Manages sync queues

## Features

- Real-time file monitoring
- Cross-platform support (Linux + Windows)
- File versioning
- Conflict detection
- Hash-based deduplication
- Lightweight embedded database (no external DB required)

## Tech Stack

- .NET Worker Services + ASP.NET Core
- Entity Framework Core
- SQLite
- File-based storage
- GitHub Actions (CI/CD)
- Designed for learning distributed system guarantees

## Purpose

A learning project to explore file synchronization, system design, and cross-platform backend development with .NET.

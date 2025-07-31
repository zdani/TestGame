# Zombie Enemy Setup Guide

## Overview
The `ZombieEnemy.cs` script provides a complete zombie enemy that:
- Moves back and forth on platforms
- Detects platform edges to prevent falling
- Deals damage to the player on collision
- Has configurable movement and combat settings

## Setup Instructions

### 1. Create the Zombie GameObject
1. Create a new GameObject in your scene
2. Add a `SpriteRenderer` component and assign a zombie sprite
3. Add a `Rigidbody2D` component:
   - Set `Body Type` to `Kinematic` (for better control)
   - Enable `Freeze Rotation Z` to prevent spinning
4. Add a `Collider2D` component (BoxCollider2D or CapsuleCollider2D)
5. Add the `ZombieEnemy` script

### 2. Configure the Script
The script has several configurable settings in the Inspector:

#### Movement Settings:
- **Move Speed**: How fast the zombie moves (default: 2)
- **Ground Check Distance**: How far to check for ground ahead (default: 0.1)
- **Wall Check Distance**: How far to check for walls ahead (default: 0.1)
- **Ground Layer**: Which layer to treat as ground/platforms

#### Combat Settings:
- **Damage Amount**: How much damage the zombie deals (default: 10)
- **Damage Cooldown**: Time between damage instances (default: 1 second)

#### References:
- **Ground Check**: Transform for ground detection (auto-created if null)
- **Wall Check**: Transform for wall detection (auto-created if null)

### 3. Layer Setup
1. Create a "Ground" layer for your platforms
2. Set the `Ground Layer` field in the ZombieEnemy script to this layer
3. Make sure your platforms are on the Ground layer

### 4. Player Setup
Ensure your player GameObject has:
- The `Player` script attached
- A `Collider2D` component
- The `Player` tag (optional but recommended)

### 5. Testing
1. Place the zombie on a platform
2. The zombie should automatically move back and forth
3. When it touches the player, the player should take damage
4. Use the Scene view with Gizmos enabled to see the ground/wall detection rays

## Features

### Platform Detection
- Uses raycasts to detect ground edges and walls
- Automatically turns around when reaching platform edges
- Prevents the zombie from falling off platforms

### Collision Detection
- Detects collision with the player using `OnCollisionEnter2D` and `OnCollisionStay2D`
- Implements damage cooldown to prevent rapid damage
- Integrates with the existing `HealthManager` system

### Visual Debugging
- Green ray shows ground detection
- Red ray shows wall detection
- Enable Gizmos in Scene view to see these rays

### Public Methods
The script provides several public methods for external control:
- `SetMoveSpeed(float speed)`: Change movement speed
- `SetDamageAmount(float damage)`: Change damage amount
- `SetDamageCooldown(float cooldown)`: Change damage cooldown
- `StopMovement()`: Stop the zombie
- `ResumeMovement(float speed)`: Resume movement

## Troubleshooting

### Zombie falls off platform:
- Check that platforms are on the correct layer
- Adjust `Ground Check Distance` if needed
- Ensure the ground check point is positioned correctly

### Zombie doesn't move:
- Check that `Move Speed` is greater than 0
- Ensure the Rigidbody2D is set to Kinematic
- Verify the zombie has a Collider2D

### Player doesn't take damage:
- Ensure the player has the `Player` script attached
- Check that the player has a Collider2D
- Verify the damage cooldown isn't too long

### Zombie gets stuck:
- Adjust `Wall Check Distance` if needed
- Check for overlapping colliders
- Ensure the wall check point is positioned correctly 
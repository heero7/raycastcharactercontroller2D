# RaycastCharacterController2D

## Description
This controller is taken straight from [Sebastian Lague's tutorial series](https://youtube.com/playlist?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz). I only implemented episodes 1 - 8 in the controller, but took the necessary controller code from episodes 9, 10, & 12. I wanted to make this controller reusable for other projects, so the player / game specifc items like wall jumping, variable jump height, and camera were all options I left up to the game. Max slopes wasn't something I was particularly interested in, so I didn't do that either.

## Requirements
- Enable `Auto Sync Transforms`
- Enable `Reuse Collision Callbacks`
- Add the "Through" game tag for moving platforms

## What can this do?
This essentially is like the 3D Character Controller that Unity ships. It will handle slopes and collisions all WITHOUT a `RigidBody2D`. So this allows you to not have that clunky feeling when using the methods from a `RigidBody2D`. Some added functionality has been added like Moving platforms and 2 Way platforms. 

## Simple Use Case
- Attach this script to your 2D `GameObject`
- Determine what your "Ground" is and give it that layer in the `CollisionMask` value <br/>(**__DON'T SET THE COLLISION LAYER TO THE SAME LAYER THE GAME OBJECT IS ON__**).
- To move anything just call `RaycastController2D.Move()`! 👏🏿

## Installation
1. Via package manager (use [UPM Git Extension](https://github.com/mob-sakai/UpmGitExtension) to always get the latest)
2. Pull down this code and create a unity package
3. Copy code to project after git pull'ing

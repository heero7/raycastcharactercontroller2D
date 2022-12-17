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


## Quality of Life Improvements
1. **Needed GameObject Tags**: It would be nice to add an editor script that could make a tag for you. If this project is pushed as a package, then it won't have the tag you need for moving platforms and could forget _unless_ you remember to read the README.
- [x] Create an Editor Script that creates the necessary game tags when this project is loaded
2. **Necessary Project Level Settings**: There are a few settings that are absolutely necessary when using this project / package. 
<br/> `Auto Sync Transforms`
<br/> `Reuse Collision Callbacks`
Withouth these you'll get some very incosistent undesired behavior. (i.e. Jitters when an entity is moving with velocity, yuck 🤮). Goal is to be able to enable this on project load. Check if these aren't set, then just set them so we don't have to remember to. 
- [x] Should enabling this project setting be an editor script?
- [x] If it can't be, how can we solve it with a GameObject?
3. Is this possible to add this to a package manager of some sort that we can just pull from? For example, instead of pulling this repo copying scripts / exporting to a package, we could just post to a local package manager and just get new versions that are published 🤷🏿‍♂️
- [ ] Explore pushing this to a package so we can just bring in updates if this changes!

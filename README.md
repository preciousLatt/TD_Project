# TD_Project

## Names: Arshiya Shahbazpourtazehaknd 100832558 & Saw Latt 100755966
![alt text](Gantt%20chart%20.png)
## Interactive media scenario information:

A Tower Defence Game with a controllable character (WASD) who can cast spells, place towers, sell towers, and remap key bindings for movement. 

The scenario's purpose will is to showcase all the design patterns so far done in class. The scene so far does not follow the player's movement but is more than capable of showcasing all the design patterns we have learned in class. Enemies will make their way to the nexus and the player must right-click to attack or cast spells to destroy the enemies and place towers to defend the objective. Towers can be upgraded and killing enemies gives cash. 

## Post-Midterm-Progress (Final)

### Performance Profiling of Object pooling vs Without Object Pooling

The idea is simple, we instantiate objects via traditional and tedious way that accumulates garbage memory and use Unity's profiling tool to compare with object pooling's performance. In our game, we use object pooling to better our performance of spawning the environment - a random forest at game start. Due to the nature of the game, it is difficult to gauge performance with little objects and projectiles in play, so we exasperated the material being spawned (8000 trees) to grasp performance profiling.

![alt text](Inst1.jpg)

In this profiling, we see 0.7 MB of GC Alloc when using the traditional for-loop and built-in Instantiate function. We first check if object pooling is enabled, which by default is not. 

![alt text](Code1.png)

However, comparing with using the object pooling design pattern, we see a much better performance compared to that of the regular Instantiate method. 

![alt text](pool1.jpg)

Here, from the start we see an inital GC Alloc of only 2.1 KB on the same 8000 test prefabs to spawn our random forest. We also included a function to clear the forest and pull from the object pool again to create it and we notice...

![alt text](pool2.jpg)

A GC Alloc of ~0 KB. This is evident when object pooling setting is checked to enabled such that the performance profiling reflects a positive increase in performance.

## Diagrams



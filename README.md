Thank you very much for the opportunity to complete this test assignment.

The project includes several configuration files for general game balance settings. They are already pre-configured to my preference, but please feel free to experiment with the values in `GamePropertiesConfig` and `MoneyConfig`. In the latter, you can adjust the spawn probability of specific notes and their denominations.

### Dependency Injection
A lightweight ServiceLocator is used for dependency injection in this project. Although this pattern is often criticized in the engineering community and labeled an anti-pattern, I deliberately chose it over heavyweight frameworks like Zenject due to the project's scale. I believe that in this specific case, ServiceLocator prevents tight coupling, scattered singletons, and/or God Objects. All global systems are registered strictly in one entry point within `GameBootstrapper`, while game states receive their dependencies via constructor injection.

### Game State Management
The core game loop is managed using the Finite State Machine pattern. Currently, there are two implemented game states: `MenuState` and `GameplayState`.

`MenuState` simply waits for user input (a click) and transitions the game to `GameplayState`.

`GameplayState` acts as the orchestrator of the match lifecycle. It is responsible for initializing abstract systems and tying them together. Its workflow can be divided into several stages:
* **Initialization:** The state constructor handles the pre-warming of object pools for cars, banknotes, and visual effects. Traffic and bot spawners are also configured here.
* **State Entry:** Entering the state triggers the spawning of participants (the player and bots) and freezes their movement.
* **Async Match Flow:** By utilizing UniTask, the game scenario is structured linearly based on the Orchestrator pattern. The main asynchronous method manages the sequence, calling logical blocks one after another. First, it triggers the countdown phase method, during which vehicle movement remain frozen. Once the timer completes, the orchestrator calls the gameplay start method, which unfreezes the vehicles and activates the traffic and notes spawners.
* **Disposal:** Upon exiting the state, it guarantees the cleanup of score models, stops all timers, and returns all entities to their respective pools, preparing a clean scene for the next session.

### Object Generation System
The object generation system is divided into two types due to differences in their behavior. The participant spawner (for the player and bots) triggers exactly once at the start of the match. It places the cars at their starting positions, injects the necessary control strategies, and registers them in the scoring system. Conversely, the traffic and notes spawners operate as continuous background processes. They are activated when the gameplay timer starts and cyclically retrieve objects from the pool.

### Vehicle Control Strategies
A shared codebase is used for both the player and the bots. The difference in their behavior is achieved by injecting the appropriate control strategy at the time of spawning. 

Currently, the project implements 3 strategies:
* **PlayerControlStrategy:** Requires a dependency in the form of the `IInputProvider` interface. (The input reading system itself is also implemented via the Strategy pattern).
* **BotControlStrategy:** Uses its own internal FSM under the hood to simulate the actions of real players, compete for banknotes, and attempt to evade traffic.
* **NpcControlStrategy:** Implements the generation of a curve across the playing field and handles the iteration along this route.

The bot and traffic strategies require a dependency in the form of the `IPlaygroundBounds` interface, which provides the necessary methods for interacting with the play area.

### Vehicle Architecture
The vehicle architecture itself is built on the principle of separation of concerns. Each car prefab consists of 3 components:
* **CarController:** The main script of the vehicle that implements several narrow interfaces: `IVehicle` for interacting with control strategies, `IMoneyCollector` for the economy and scoring system, and `IPoolable` for memory management. It is also responsible for the safe lifecycle of the object, resetting subscriptions, timers, and the motor state when returned to the pool.
* **VehicleMotor:** Handles locomotion, acceleration calculations, collision physics simulation, impulse application, and keeping the car within the arena boundaries.
* **CarVisuals:** Manages the visual effects of the vehicle, such as tire smoke, skid marks, and collision effects. For accurate implementation of these visual effects, it queries the `CarController` for the `CarTelemetry` struct, which contains necessary data like speed, angular velocity, turn direction, etc.

### Custom Physics Simulation
The project does not use Unity's built-in physics engine. Instead, an independent mathematical collision simulation has been implemented. The system consists of 2 main elements:
* **MathCollider:** A spherical collider component. It stores data regarding the object's radius, a trigger flag, and layer settings. Upon being enabled in the scene, it automatically registers itself with the collision manager.
* **MathCollisionManager:** The central calculator of the system. It iterates through the list of all active colliders and mathematically checks for intersections using `sqrMagnitude` to avoid trigonometric calculations in every frame. If two solid objects (non-triggers) collide, the manager independently calculates the penetration depth, generates a correction vector, and sends it to the colliding entities.

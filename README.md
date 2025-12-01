# Smart-Home-Simulation
This project demonstrates an interactive smart home automation system by integrating virtual simulation, IoT hardware emulation, and cloud-based data monitoring. 
Motion data is generated using Wokwi, where virtual sensors simulate real-time PIR motion detection.
The data is sent to ThingSpeak, which acts as a cloud IoT platform for storing and visualizing sensor values. Unity is then connected to ThingSpeak through a C# script, allowing the 3D environment to react dynamically to live sensor updates. 
When motion is detected, the light inside the Unity smart home automatically turns on, and switches off when no motion is present. 
This system showcases seamless communication between IoT components and a virtual environment, making it ideal for smart home demonstrations, interactive simulations, and educational IoT experiments.

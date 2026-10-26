**Star Wars C Sharp Project Overview**

This is the project I built for the C# Backend course for Code Louisville. I wanted to keep my project simple and fun, so I created a program that pulls data from a HUGE
StarWars API! I am a huge nerd in general and a lot of people like Star Wars so I thought it would be fun to create a project around those parameters. 


//These are the instructions for the StarWars Program I built.

Step 1:
Make sure you download the lastest version of Visual Studio for your PC. If you are on a Mac you will need to use this on a PC that has Visual studio on it, or on Mac running a virtual machine/ parallels/ dual booted copy of windows. Then download or run visual studio in order to open the project files.

Step 2:
Open the folder in Visual Studio or click on the sln. file (aka solution file) and Visual studio will open the project.

Step 3:
In order to run the project click the start button with the green arrow.

Step 4:
Follow the instructions giving in the commandline and enjoy :)


**2026 update**

The original Star Wars API (swapi.co) shut down, so the project now uses https://swapi.dev/api. Both projects target .NET Framework 4.8, and the JSON library was updated for a security fix. If Visual Studio says a package is missing, right-click the solution and choose "Restore NuGet Packages."

If swapi.dev is ever down, set the SWAPI_BASE_URL environment variable to another mirror, e.g. `set SWAPI_BASE_URL=https://swapi.py4e.com/api`, before running.

See SECURITY_UPDATES.md for every change and why. 


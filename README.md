# image-maze-solver

C# program that solves mazes from pictures, even low-quality ones

## Overview

Image Maze Solver is a .NET application designed to solve mazes from uploaded images. The user can choose the start and end points within the maze. The solution leverages Lee's algorithm and custom functions to detect walls based on pixel color differences, even in low-quality pictures.

## Features

-Upload an image of a maze.

-Select start and end points within the maze.

-Solve the maze using Lee's algorithm.

-Detect walls and paths even in low-quality images through advanced pixel color difference analysis.

## How to use?

-Clone the repository:


```git clone https://github.com/dorufloare/image-maze-solver.git```

```cd image-maze-solver```

Open the solution file in Visual Studio.

Restore the required packages:

```dotnet restore```

Build the project:

```dotnet build```

Run the application:

```dotnet run```

## Usage

Upload an Image: Upload an image of the maze you want to solve. Ensure the image clearly shows the maze's walls and paths.

Select Start and End Points: Use the UI to select the start and end points within the maze.

Solve the Maze: Click the "Solve" button to start the solving process. The application will display the solution path from the start to the end point.

## How It Works

-Lee's Algorithm

  -Lee's algorithm, a breadth-first search algorithm, is used to find the shortest path in the maze. It is well-suited for grid-based pathfinding.

-Pixel Color Difference Detection

  -The application includes custom functions to analyze pixel color differences. This allows the solver to accurately detect walls and paths, even in some low-quality images where the distinction between walls and paths might not be clear.

## Steps

-Image Processing: The uploaded image is processed to identify potential walls and paths.

-Maze Grid Creation: The processed image is converted into a grid representation of the maze.

-Pathfinding: Lee's algorithm is applied to the grid to find the shortest path from the start to the end point.

-Solution Display: The solution path is overlayed on the original image and displayed to the user.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

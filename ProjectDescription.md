# Project Description

## Game mechanics
This project is a desktop game called Minesweeper, made with C# and WPF. It is a classic logic game where the player needs to open all safe cells on the board without clicking on a mine. The game also includes flags, a timer, difficulty levels, and three lives, which makes it a little different from the original version.
The game creates a square board made of buttons. Some of these cells contain mines, but the player cannot see them at the beginning.
* Goal: The goal is to open all cells that do not contain mines. 
* Lives: If the player clicks on a cell with a mine, they lose one life. When all three lives are lost, the game is over. If the player opens all safe cells before losing all lives, they win.
* Flags: The player can also use the right mouse button to place a flag on a cell. Flags help the player remember where mines may be located.
* The game also shows how many mines are still left and how much time has passed since the first click.

## How it works
When the program starts, it creates a new game field and randomly places mines on it. After that, it calculates how many mines are around each cell. This number is shown when the player opens a cell.
* If a cell has no mines near it, the game automatically opens the nearby cells too. This helps the player clear empty areas faster.
* The first click is always safe. If the first clicked cell contains a mine, the mine is moved to another place. This makes the game fair and easier to start.
* The timer starts after the first click, and it stops when the game ends.

## Three lives
This version of Minesweeper includes a three-lives system.
Normally in classic Minesweeper, one mistake can end the game immediately. In this project, the player has three chances. Every time the player opens a mine, one life is lost. The life counter is updated on the screen. Only when the player loses all three lives does the game finish completely.

# Tools and technologies
This project was created with the following tools and technologies: 
* C#-used for the game logic and event handling, WPF- used for building the window and the graphical interface.
* XAML - used to design the layout of the game window. 
* dispatcherTimer - used for the game timer.
* button controls - used for the cells on the board.

# Welcome to our implementation of tetris!

Created by Stefan and Philip

Student number Stefan: 1082868

Student number Philip: 4735781


## How to play
When starting up for the first time there will be a chunk of text explaining the menu controls, but they will also be listed here.

### Menu
- Use the up and down arrow keys to move to different buttons.
- Use the enter key to select the button.
- Some buttons have a button to their right, by using the left and right keys you can cycle through the values.
- You can press the escape key as an alternative to the back button.
- Pressing escape on the main menu exits the game
- While hovering over the create profile button, you can enter letters to give the profile a name.

### Gameplay
You can press escape to pause the game, pressing escape again will unpause.
If you press backspace while in the pause menu, the game is aborted and you go to the main menu.
The controls listed past this can be adjusted in the controls tab, either by changing the values of the default profile or making a new profile.
Multiple keys can be mapped to a single action
- Move left: left arrow key, A
- Move right: right arrow key, D
- Soft drop: down arrow key, S
- Hard drop: space
- Rotate clockwise (default rotation): up arrow key, W, E, X
- Rotate counter clockwise: Q, Z
- Hold piece: left shift, right shift

## Extra features
Lots of extra features have been added, mainly based on an old leaked document containing all the guidelines for modern tetris games.
These guidelines are implemented by most official and fan made tetris games and contain a lot of the smooth mechanics that many people enjoy.
We will be referencing the document on all the added features to give more detail to what we have added.
The document is packaged together with the project files and is named "2009 Tetris Design Guideline.pdf".

- Next queue (3. Next queue, page 6)
- Ghost piece (4. Ghost piece, page 7)
- Background graphic (5. Background graphic, page 7)
- Lines cleared, Current score, High score, Level (7. Game Information, page 7)
- Hold queue (8. Hold queue, page 7)
- Lock down flexibility (2.5.4 Lock down, page 8 and 5.7 Extended placement lock down, page 17)
- 'Balanced' random generation (3.3 Random generation, page 9)
- Dynamic starting location (3.4 Starting location & orientation, page 9) (our version also works with different width matrices, hence 'dynamic')
- Auto repeat (5.2 Auto repeat, page 16)
- Super Rotation System (5.3 Rotation, page 16 and A1.4 Super Rotation System, page 36) (This is different from NES tetris rotation)
- Hard drop (5.4 Hard drop, page 16)
- Soft drop (5.5 Soft drop, page 17)
- Hold piece (5.6 Hold, page 17)
- Line clear scoring (6.1 Variable goal system & line clears, page 19) (more complicated actions award more lines (this is also what the LINES stat in the game means, so yes a tetris awards 8 lines))
- Level based falling speeds (7 Fall & drop speeds, page 20)
- Level based soft drop speed (7.1 drop speeds, page 20)
- Variable and level based scoring (8 Scoring, page 21) (Back-to-backs and (mini-)t-spins are also detected)
- T-spins (9 T-spins, page 22) (Yes all these cases are acounted for)
- Correct game over detection (10 Game over conditions, page 26)
- Line clear effect (13.1.1 Line clear effects, page 30)
- Lock down effect (13.1.1 Lock down, page 30)
- Action notifications (13.1.5 Action notifications, page 30)
- Correct flow of checks in code (Appendix A The tetris engine, page 33) (Includes all the Phases as listed with exeption of the iterate phase as it has no usage in our version)
- Split screen multiplayer (Appendix B Multiplayer, page 79)

(No longer referencing the design document)
- Tug of war variant, this is our custom made multiplayer mode (versus being the official). 
- You both play your own game and the first person to have cleared 30 (unless changed in settings) lines more that the opponent wins (indicated by the bar above the screens).
- The ability to change the width of the matrix/board.
- An entire menu to change settings, control profiles and game modes.
- The game automatically saves settings and the high score to a file.
- Set profiles to play with the keybinds you want, or to split up the keyboard for multiplayer. 

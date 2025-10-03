namespace TetrisProject.Machine_Learning;

public class NeuralNetworkManager
{
    /* Main neural network training idea:
     * Will get entire current board as true/false (0/1), and get current piece, held piece, upcoming pieces
     * Will output next 10 actions (each action will be represented with 3 nodes of 0/1, giving 8 actions, we already have 7 and then add action for nothing)
     * Give score based off of difference in score and additional potential score (so easily clearable lines) after 10 actions done
     * Penalty for losing and for differences in height from average (excluding diffs of 1)
     * Could improve training later by giving it random situations to do actions with
     */
    
    public void OnMatchStart()
    {
        
    }
}
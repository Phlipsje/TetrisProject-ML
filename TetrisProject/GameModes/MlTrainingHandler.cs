using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using TetrisProject.Machine_Learning;

namespace TetrisProject;

public class MlTrainingHandler : GameHandler
{
    NeuralNetworkManager nnManager;
    public MlTrainingHandler(ContentManager content, GameMode gameMode, Settings settings, List<Controls> controls, Main mainReference = null) : base(content, gameMode, settings, controls, mainReference)
    {
        tetrisGames.Add(new TetrisGame(this, settings, controls[0], gameMode));
        nnManager = new NeuralNetworkManager();
    }
}
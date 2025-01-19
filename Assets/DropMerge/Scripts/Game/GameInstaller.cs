using GameFoundation.FSM;
using UnityEngine;
using Zenject;

namespace DropMerge.Game
{
    public class GameInstaller : MonoInstaller
    {
        public GameSetting setting;
        public Spawner spawner;
        public Camera captureCamera;
        public GameAudio gameAudio;

        public override void InstallBindings()
        {
            var gameModel = new GameModel();
            var fsm = new StateMachine();
            Container.BindInstances(setting, gameModel, fsm, spawner, captureCamera, gameAudio);
        }
    }
}

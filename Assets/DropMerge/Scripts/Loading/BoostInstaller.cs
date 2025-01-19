using System.Collections;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace DropMerge.Loading
{
    public class BoostInstaller : MonoInstaller
    {
        public Image progressBar;
        public Game.CatAssets catAssets;

        public override void InstallBindings() { }

        public override void Start()
        {
            base.Start();
        }

        public async void OnBoostComplete()
        {
            // create player model
            var playerModel = new PlayerModel();
            ProjectContext.Instance.Container.BindInstances(catAssets, playerModel);

            // load scene
            var loadSceneOperation = SceneManager.LoadSceneAsync(Scenes.Game);
            loadSceneOperation.allowSceneActivation = false;

            await Tween.UIFillAmount(progressBar, 1, 1);

            // update player setting
            playerModel.UpdateSetting();

            Debug.Log("Replace");
            loadSceneOperation.allowSceneActivation = true;
        }
    }
}

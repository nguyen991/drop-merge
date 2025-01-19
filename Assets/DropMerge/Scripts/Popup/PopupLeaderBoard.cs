using System.Collections;
using System.Collections.Generic;
using DropMerge.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DropMerge.Popup
{
    public class PopupLeaderBoard : MonoBehaviour
    {
        enum TankTab
        {
            Today,
            Week,
            All
        }

        public Sprite[] itemSprites;
        public Transform content;
        public GameObject itemPrefab;
        public Button leaderBoardButton;
        public ToggleGroup tabGroup;

        private int currentTab = -1;

        [Inject]
        private void Inject(CatAssets catAssets)
        {
            // init board
            for (var i = 0; i < itemSprites.Length; i++)
            {
                var item = Instantiate(itemPrefab, content);
                item.GetComponent<Image>().sprite = itemSprites[i];
                item.transform.Find("Icon").GetComponent<Image>().sprite = catAssets
                    .GetCatData(catAssets.MaxCatId - i)
                    .icon;
                item.transform.Find("Index").GetComponent<TextMeshProUGUI>().text = (
                    i + 1
                ).ToString();
            }
            itemPrefab.SetActive(false);
            leaderBoardButton.onClick.AddListener(() => Debug.Log("Show LeaderBoard"));
        }

        public void OnPopupShow()
        {
            OnChangeTab(true);
        }

        public void OnChangeTab(bool isOn)
        {
            if (!isOn)
            {
                return;
            }

            var index = tabGroup.GetFirstActiveToggle().transform.GetSiblingIndex();
            if (index == currentTab)
            {
                return;
            }

            Debug.Log("Change tab: " + (TankTab)index);
            currentTab = index;
            foreach (Transform child in content)
            {
                child.Find("Score").GetComponent<TextMeshProUGUI>().text = Random
                    .Range(100, 1000)
                    .ToString("N0");
            }
        }
    }
}

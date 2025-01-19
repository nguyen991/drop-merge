using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Storage
{
    public class GameStorage : SingletonBehaviour<GameStorage>
    {
        public enum DataLayerType
        {
            None,
            PlayerPrefs,
            File,
            Cloud
        }

        private IDataLayer dataLayer;

        private readonly Dictionary<string, object> autoSaveData = new();

        public void SetDataLayer(DataLayerType type, object option)
        {
            dataLayer = type switch
            {
                DataLayerType.PlayerPrefs => new PlayerPrefsLayer(),
                _ => null,
            };
        }

        public void AutoSave(string key, object value)
        {
            if (autoSaveData.ContainsKey(key))
            {
                autoSaveData[key] = value;
            }
            else
            {
                autoSaveData.Add(key, value);
                Load(key, value);
            }
        }

        public void Save(string key, object value)
        {
            dataLayer.Save(key, value);
        }

        public void Load<T>(string key, T overrideValue)
        {
            dataLayer.Load(key, overrideValue);
        }

        public T Load<T>(string key)
        {
            return dataLayer.Load<T>(key);
        }

        public void Delete(string key)
        {
            dataLayer.Delete(key);
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (!focusStatus)
            {
                Debug.Log("OnApplicationFocus: false, saving data...");
                foreach (var data in autoSaveData)
                {
                    Save(data.Key, data.Value);
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("OnApplicationPause: true, saving data...");
                foreach (var data in autoSaveData)
                {
                    Save(data.Key, data.Value);
                }
            }
        }
    }
}

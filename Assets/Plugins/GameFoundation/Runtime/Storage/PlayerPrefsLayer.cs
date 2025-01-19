using Newtonsoft.Json;
using UnityEngine;

namespace GameFoundation.Storage
{
    class PlayerPrefsLayer : IDataLayer
    {
        public void Save(string key, object value)
        {
            // serialize the object to a JSON string with Newtonsoft.Json
            string json = JsonConvert.SerializeObject(value);
            Debug.Log(json);
            PlayerPrefs.SetString(key, json);
        }

        public T Load<T>(string key)
        {
            // get the JSON string from PlayerPrefs
            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }
            Debug.Log(json);

            // deserialize the JSON string to an object with Newtonsoft.Json
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Load<T>(string key, T overrideValue)
        {
            // get the JSON string from PlayerPrefs
            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            Debug.Log(json);

            // deserialize the JSON string to an object with Newtonsoft.Json
            JsonConvert.PopulateObject(json, overrideValue);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}

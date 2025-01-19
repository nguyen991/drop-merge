namespace GameFoundation.Storage
{
    interface IDataLayer
    {
        void Save(string key, object value);

        void Delete(string key);

        T Load<T>(string key);

        void Load<T>(string key, T overrideValue);
    }
}

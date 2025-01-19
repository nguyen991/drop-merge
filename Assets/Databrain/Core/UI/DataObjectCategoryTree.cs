using System.Collections.Generic;
using System.Linq;

namespace Databrain.Core.UI
{
    public class DataObjectCategoryTree
    {
        
        public Dictionary<string, DataObjectCategoryTree> categories = new();
        public List<DataObject> dataObjectsInCategory = new();
        public DataObjectCategoryTree parentGraphTree;

        public string Path = "";
        public string CompletePath = "";

        public DataObjectCategoryTree() { }
        public static DataObjectCategoryTree CollectNodes(List<DataObject> _availableObjects, string _filter = "")
        {
            DataObjectCategoryTree collectedObjects = new();

            List<DataObject> collected = _availableObjects.OrderBy(c => c.title).ToList();


            for (int c = 0; c < collected.Count; c++)
            {
                var _child = collectedObjects.BuildTree(collected[c].title);

                _child.dataObjectsInCategory.Add(collected[c]);

            }

            return collectedObjects;
        }
        
        public DataObjectCategoryTree BuildTree(string _title)
        {
            // Parse into a sequence of parts.
            string[] parts = _title.Split("/");
            string _name = parts[^1];
            string _path = parts.Length>0?parts[0]:"";

            // The current tree.  Start with this.
            DataObjectCategoryTree current = this;
            // Iterate through the parts.
            for (int i = 0; i < parts.Length -1; i++, _path += "/" + parts[i])
            {
                // The child GraphTree.
                DataObjectCategoryTree child;

                // Does the part exist in the current GraphTree?  If
                // not, then add.
                if (!current.categories.TryGetValue(parts[i], out child))
                {
                    // Add the child.
                    child = new DataObjectCategoryTree
                    {
                        Path = parts[i],
                        CompletePath = _path,
                        parentGraphTree = current
                    };
                    ;

                    // Add to the dictionary.
                    current.categories[parts[i]] = child;
                }
                

                // Set the current to the child.
                current = child;
            }

            return current;

        }
        
        public List<DataObjectCategoryTree> GetAllChildsCategories()
        {
            List<DataObjectCategoryTree> _childs = new();
            foreach (var category in categories)
            {
                _childs.Add(category.Value);
                _childs.AddRange(category.Value.GetAllChildsCategories());
            }

            return _childs;
        }
        
        public List<DataObject> GetAllChildsDataObjects()
        {
            List<DataObject> _dataObjects = new();
            _dataObjects.AddRange(dataObjectsInCategory);
            foreach (var category in categories)
            {
                _dataObjects.AddRange(category.Value.GetAllChildsDataObjects());
            }

            return _dataObjects;
        }
    }
}
/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using Databrain.Logic.Utils;
using System.Threading.Tasks;

namespace Databrain.Logic
{
    public class NodeAssetCreator : AssetPostprocessor
    {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {

                var _asset = (CreateNodePlaceholder)AssetDatabase.LoadAssetAtPath(str, typeof(CreateNodePlaceholder));


                if (_asset != null && _asset.GetType() == typeof(CreateNodePlaceholder))
                {

                    nodeName = Path.GetFileNameWithoutExtension(str);

                    nodePath = Path.GetDirectoryName(str);
                    nodePath = nodePath.Replace(@"\", "/");
                    
                    // Delete simple node placeholder asset
                    AssetDatabase.DeleteAsset(str);

                    // Create new simple node script based on placeholder name
                    Create();

                    AssetDatabase.Refresh();
                }
            }

            foreach (string str in importedAssets)
            {

                var _asset = (CreateStateActionNodePlaceholder)AssetDatabase.LoadAssetAtPath(str, typeof(CreateStateActionNodePlaceholder));


                if (_asset != null && _asset.GetType() == typeof(CreateStateActionNodePlaceholder))
                {

                    nodeName = Path.GetFileNameWithoutExtension(str);

                    nodePath = Path.GetDirectoryName(str);
                    nodePath = nodePath.Replace(@"\", "/");
                    
                    // Delete simple node placeholder asset
                    AssetDatabase.DeleteAsset(str);

                    // Create new simple node script based on placeholder name
                    CreateFSMStateNode();

                    AssetDatabase.Refresh();
                }
            }
        }


        static string nodeTemplate;
        static string nodeName;
        static string nodePath;

        static void Create()
        {

            nodeTemplate = System.IO.Path.Combine(LogicHelpers.GetRelativeResPath(), "nodeTemplate.txt");


            StreamReader sr = new StreamReader(nodeTemplate);
            var _content = sr.ReadToEnd();

            sr.Close();

            nodeName = nodeName.Trim();



            // Replace all node properties			
            ///////////////////////////////

            if (_content.Contains("NODE_TITLE"))
            {
                _content = _content.Replace("NODE_TITLE", '"' + nodeName + '"');
            }

            if (_content.Contains("NODE_NAME"))
            {
                _content = _content.Replace("NODE_NAME", nodeName);
            }


            // Remove white spaces
            nodeName = nodeName.Replace(" ", "_");
            nodeName = nodeName.Replace("-", "_");

            StreamWriter sw = new StreamWriter(System.IO.Path.Combine(nodePath, (nodeName + ".cs")));

            sw.Write(_content.Replace("\r\n", "\n"));
            sw.Close();


            Refresh();
        }


        static void CreateFSMStateNode()
        {

            nodeTemplate = System.IO.Path.Combine(LogicHelpers.GetRelativeResPath(), "stateActionNodeTemplate.txt");


            StreamReader sr = new StreamReader(nodeTemplate);
            var _content = sr.ReadToEnd();

            sr.Close();

            nodeName = nodeName.Trim();



            // Replace all node properties			
            ///////////////////////////////

            if (_content.Contains("NODE_TITLE"))
            {
                _content = _content.Replace("NODE_TITLE", '"' + nodeName + '"');
            }

            if (_content.Contains("NODE_NAME"))
            {
                _content = _content.Replace("NODE_NAME", nodeName);
            }


            // Remove white spaces
            nodeName = nodeName.Replace(" ", "_");
            nodeName = nodeName.Replace("-", "_");

            StreamWriter sw = new StreamWriter(System.IO.Path.Combine(nodePath, (nodeName + ".cs")));

            sw.Write(_content.Replace("\r\n", "\n"));
            sw.Close();


            Refresh();
        }




        static async void Refresh()
        {
            await Task.Delay(1000);

            AssetDatabase.Refresh();

        }
    }
}
#endif
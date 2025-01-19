/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine;
using System.IO;

namespace Databrain.Logic
{
    [NodeTitle("Save")]
    [NodeCategory("Databrain")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeIcon("save")]
    [NodeDescription("Save runtime library to file")]
    [NodeColor(DatabrainColor.Blue)]
    public class Save : NodeData
    {
        public string fileName;

        public enum PathType
        {
            persistentDataPath,
            custom
        }

        public PathType path;


        public override void ExecuteNode()
        {
            switch (path)
            {
                case PathType.persistentDataPath:
                    this.relatedLibraryObject.Save(Path.Combine(Application.persistentDataPath, fileName));
                    break;
                case PathType.custom:
                    this.relatedLibraryObject.Save(fileName);
                    break;
            }
    
            ExecuteNextNode(0);
        
        }

    }
}
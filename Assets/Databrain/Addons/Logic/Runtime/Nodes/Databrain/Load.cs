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
    [NodeTitle("Load")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeCategory("Databrain")]
    [NodeIcon("refresh")]
    [NodeDescription("Load runtime library from file")]
    [NodeColor(DatabrainColor.Blue)]
    public class Load : NodeData
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
                    this.relatedLibraryObject.Load(Path.Combine(Application.persistentDataPath, fileName));
                    break;
                case PathType.custom:
                    this.relatedLibraryObject.Load(fileName);
                    break;
            }

            ExecuteNextNode(0);

        }

    }
}
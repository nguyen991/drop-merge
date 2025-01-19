#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Databrain.Helpers
{

    // Helper class which finds missing scripts in sub assets of a DataLibrary.
    // It will modify the actual local asset yaml file to remove all broken sub-assets.
    // This is unfortunately the only way to do it as Unity does not provide any direct API for it.
    public class AssetDatabaseHelper
    {
        public readonly static Regex LocalIdPattern = new Regex(@"---\s!.!\d+\s&(.*)");


        public static void FixMissingScripts(Object mainAsset)
        {
            EditorApplication.delayCall += () =>
            {
                var ownerPath = AssetDatabase.GetAssetPath(mainAsset);
                string tempFile = Path.GetTempFileName();

                var hasMissingScripts = false;
                var _x = ParseSubAssets(mainAsset, out hasMissingScripts);

                if (hasMissingScripts)
                {
                    using (var sw = new StreamWriter(tempFile))
                    {
                        var yaml = string.Join("", ParseSubAssets(mainAsset, out hasMissingScripts).Select(x => x.Item2).ToArray());
                        sw.Write(yaml);        
                    }

                    File.Delete(ownerPath);
                    File.Move(tempFile, ownerPath);
                    

                    AssetImporter assetImporter = AssetImporter.GetAtPath(ownerPath);
                    EditorUtility.SetDirty(assetImporter);
                }
            

                if (hasMissingScripts)
                {
                    AssetDatabase.ImportAsset(ownerPath);
                    Debug.Log("DATABRAIN - Databrain has detected sub-assets with missing script files. Null sub-assets have been cleaned up. Please ignore asset file warnings, as they appear because of the file modification and re-import.");
                }
            };
        }

    public static Object[] GetAllSubAssets(Object mainAsset)
    {
        Object[] allSubAssets = null;
        bool hasMissingScripts = false;
        var yamlIdentifiers = ParseSubAssets(mainAsset, out hasMissingScripts);
        var yamlAssets = yamlIdentifiers.ToArray();
 
        var assetPath = AssetDatabase.GetAssetPath(mainAsset);
        allSubAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(x => x != mainAsset).ToArray();


        var subAssets = new List<Object>(); 
        var subIndex = 0;
        foreach (var yamlAsset in yamlAssets)
        {
            try
            {
                var subAsset = allSubAssets[subIndex];
                if (AssetDatabase.IsSubAsset(subAsset)) subAssets.Add(subAsset);
            }
            catch (System.Exception)
            {
            }
            subIndex++;
        }
 
        return subAssets.ToArray();
    }

        private static (long, string)[] ParseSubAssets(Object mainAsset, out bool hasMissingScripts)
        {
            hasMissingScripts = false;
            var ownerPath = AssetDatabase.GetAssetPath(mainAsset);
            _ = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mainAsset, out _, out long mainAssetLocalId);
     
            var yaml = new List<(long, StringBuilder)>();
            // var index = indices?[0];
            List<long> missingLocalIds = new List<long>();

            List<long> localIds = new List<long>();

            // First collect all local ids so we can later
            // check them against the fileIDs in the yaml.
            using (var sr = new StreamReader(ownerPath))
            {
               
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var match = LocalIdPattern.Match(line);
                    if (match.Success)
                    {
                        var localId = long.Parse(match.Groups[1].Value);
                        
                        if (!localIds.Contains(localId))
                            localIds.Add(localId);
                    }
                }
            }

            using (var sr = new StreamReader(ownerPath))
            {
                var line = string.Empty;
                // var delete = false;
                var subAssetIndex = 0;
                var localId = 0L;
     
                while ((line = sr.ReadLine()) != null)
                {
                    var match = LocalIdPattern.Match(line);
                    
                    if (match.Success)
                    {
                        localId = long.Parse(match.Groups[1].Value);
                        var isSubAsset = localId != mainAssetLocalId;
                      
                        subAssetIndex++;

                    }

                    // if (!delete)
                    // {

                        // Fix missing/null sub-assets
                        if (line.Contains("m_Script:"))
                        {
                            ICollection<string> matches =
                            Regex.Matches(line.Replace(System.Environment.NewLine, ""), @"{(.*?)}")
                                .Cast<Match>()
                                .Select(x => x.Groups[1].Value)
                                .ToList();

                            var _splitted = matches.ToArray()[0].Split(",");

                            try
                            {
                                var _guid = _splitted[1].Replace("guid:", "").Trim(new char[]{' '});
                                
                                var _scriptFile = AssetDatabase.GUIDToAssetPath(_guid);
                                if (string.IsNullOrEmpty(_scriptFile))
                                {
                                    if(!missingLocalIds.Contains(localId))
                                    {
                                        missingLocalIds.Add(localId);
                                    }
                                }
                            }
                            catch{}

                        }

                        // Fix missing asset file id references.
                        // Happens if a custom DataObject or asset reference doesn't exist anymore.
                        if (line.Contains("fileID:"))
                        {
                            try
                            {
                                ICollection<string> matches =
                                Regex.Matches(line.Replace(System.Environment.NewLine, ""), @"{(.*?)}")
                                    .Cast<Match>()
                                    .Select(x => x.Groups[1].Value)
                                    .ToList();

                                if (matches.Count > 0)
                                {
                                    var _splitted = matches.ToArray()[0].Split(",");

                                    try
                                    {
                                        if (_splitted.Length == 1)
                                        {
                                            var _fileID = _splitted[0].Replace("fileID:", "").Trim(new char[]{' '});

                                            if (!localIds.Contains(long.Parse(_fileID)) && _fileID != "0")
                                            {
                                                line = line.Replace("{fileID: " + _fileID + "}", "{fileID: 0}");
                                                hasMissingScripts = true;
                                            }
                                        }
                                    }
                                    catch{}
                                }
                            }catch{}
                        }

                        if (subAssetIndex >= yaml.Count)
                        {
                             yaml.Add((localId, new StringBuilder()));
                            
                        }

                        if (subAssetIndex < yaml.Count)
                        {
                            yaml[subAssetIndex].Item2.Append(line + System.Environment.NewLine);
                        }
                        else
                        {
                            yaml[subAssetIndex-1].Item2.Append(line + System.Environment.NewLine);
                        }
                    // }

                }
            }

            
            // foreach(var x in missingLocalIds)
            // {
            //     Debug.Log("remove missing: " + x);
            // }

            // remove sub-assets ids from yaml
            missingLocalIds.ForEach(x => yaml.RemoveAll(y => y.Item1 == x));

            if (missingLocalIds.Count > 0)
            {
                hasMissingScripts = true;
            }
     
            return yaml.Select(x => (x.Item1, x.Item2.ToString())).ToArray();
        }
    }
     
}
#endif
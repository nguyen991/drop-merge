﻿/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using Databrain.Data;
using Databrain.Attributes;
using Databrain.Helpers;
using System.Linq;

namespace Databrain
{
	public class  DataObjectCreator
	{
		public static DataObject CreateNewDataObject(DataLibrary _dataLibrary, Type _type)
		{
			ScriptableObject _dbObject = null;
			try
			{
                _dbObject = ScriptableObject.CreateInstance(_type);
			}
			catch (Exception _e)
			{
                
                Debug.LogWarning(_e);
				return null;
			}

			(_dbObject as DataObject).guid = System.Guid.NewGuid().ToString();
			(_dbObject as DataObject).initialGuid = (_dbObject as DataObject).guid;
            (_dbObject as DataObject).name = (_dbObject as DataObject).title + " / " + _type.Name;
			(_dbObject as DataObject).relatedLibraryObject = _dataLibrary;

			(_dbObject as DataObject).Reset();

			if (_dataLibrary.hideDataObjectsByDefault)
			{
				_dbObject.hideFlags = HideFlags.HideInHierarchy;
			}
			else
			{
				_dbObject.hideFlags = HideFlags.None;
			}

			// Commented out because this can cause an issue when HideFlags of sprites are set to DontSave.
			// Then builds will fail.

			// var _iconAttribute = _type.GetCustomAttribute(typeof(DataObjectIconAttribute)) as DataObjectIconAttribute;
			// if (_iconAttribute != null) 
			// {
			// 	var _icon = DatabrainHelpers.LoadIcon(_iconAttribute.iconPath);
			// 	EditorGUIUtility.SetIconForObject(_dbObject, _icon);
			// }

            AssetDatabase.AddObjectToAsset(_dbObject, _dataLibrary);


			var _tagAttribute = _type.GetCustomAttribute(typeof(DataObjectDefaultTagAttribute)) as DataObjectDefaultTagAttribute;
			if (_tagAttribute != null && _dataLibrary != null)
			{
				var validTagsFromLibrary = _dataLibrary.tags;

				foreach (var tag in _tagAttribute.defaultTags)
				{
					if (validTagsFromLibrary.Contains(tag))
					{
						(_dbObject as DataObject).tags.Add(tag);
					}
					else
					{
						var _result = EditorUtility.DisplayDialog("Tag not fdoun", "The tag \"" + tag + "\" was not found in the tag library. Do you want to add it?", "Yes", "No");
						if (_result)
						{
							_dataLibrary.tags.Add(tag);
							(_dbObject as DataObject).tags.Add(tag);
							EditorWindow.GetWindow<DatabrainEditorWindow>().SetupForceRebuild(_dataLibrary, true);
							EditorUtility.SetDirty(_dataLibrary);
						}
					}
				}
			}



			EditorUtility.SetDirty(_dbObject);
			//AssetDatabase.SaveAssets();
			//AssetDatabase.Refresh();


            string assetPath = AssetDatabase.GetAssetPath(_dbObject);
            var _guidString = AssetDatabase.AssetPathToGUID(assetPath);
            GUID _guid = GUID.Generate();
            GUID.TryParse(_guidString, out _guid);
            AssetDatabase.SaveAssetIfDirty(_guid);


            DataObjectCreator.AddDataObject(_dataLibrary, _type, _dbObject as DataObject);


            return _dbObject as DataObject;
		}

		public static DataObject DuplicateDataObject(DataLibrary _dataLibrary, DataObject _duplicateFrom)
		{
            var _index = _duplicateFrom.title.IndexOf("(");
            var _newTitle = _index > -1 ? _duplicateFrom.title.Substring(0, _index - 1) : _duplicateFrom.title;

            var _objects = _dataLibrary.GetAllInitialDataObjectsByType(_duplicateFrom.GetType());
			var _existingNamesList = _objects.Where(x => x.title.Contains(_newTitle)).ToList();


			var _new = ScriptableObject.Instantiate(_duplicateFrom);

			(_new as DataObject).guid = System.Guid.NewGuid().ToString();
			(_new as DataObject).initialGuid = (_new as DataObject).guid;
		
            (_new as DataObject).title = _newTitle + " (" + (_existingNamesList.Count) + ")";
			(_new as DataObject).name = (_new as DataObject).title + " / " + _duplicateFrom.GetType().Name;  
			(_new as DataObject).relatedLibraryObject = _dataLibrary;

			if (_dataLibrary.hideDataObjectsByDefault)
			{
				_new.hideFlags = HideFlags.HideInHierarchy;
			}
			else
			{
				_new.hideFlags = HideFlags.None;
			}
	
			_new.OnDuplicate(_duplicateFrom);

            AssetDatabase.AddObjectToAsset(_new, _dataLibrary);

            EditorUtility.SetDirty(_new);
            AssetDatabase.SaveAssets();
            DataObjectCreator.AddDataObject(_dataLibrary, _duplicateFrom.GetType(), _new as DataObject);


            return _new as DataObject;
        }

		/// <summary>
		/// Editor method to add newly create data object to the data object list of the datacore object and then applying modified properties
		/// as well as setting the datacore object dirty.
		/// </summary>
		/// <param name="_datacore"></param>
		/// <param name="_dataType"></param>
		/// <param name="_dataObject"></param>
		static void AddDataObject(DataLibrary _datacore, Type _dataType, DataObject _dataObject)
        {
            if (_datacore.data == null)
                _datacore.data = new DataObjectList();


            _datacore.data.AddDataObject(_dataType, _dataObject);

            var _editor = Editor.CreateEditor(_datacore);
            _editor.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(_datacore);
        }


		// public static DataProperty CreateSubDataObject(Type _type, DataObject _dataObject, DataLibrary _dataLibrary)
		// {
		// 	ScriptableObject _dbObject = null;
		// 	try
		// 	{
        //         _dbObject = ScriptableObject.CreateInstance(_type);
		// 	}
		// 	catch (Exception _e)
		// 	{
                
        //         Debug.LogWarning(_e);
		// 		return null;
		// 	}

		// 	(_dbObject as DataProperty).guid = System.Guid.NewGuid().ToString();
		// 	(_dbObject as DataProperty).initialGuid = (_dbObject as DataObject).guid;
        //     (_dbObject as DataProperty).name = (_dbObject as DataObject).title + " / " + _type.Name;
		// 	(_dbObject as DataProperty).relatedLibraryObject = _dataObject.relatedLibraryObject;
		// 	(_dbObject as DataProperty).relatedDataObject = _dataObject;
			
		// 	(_dbObject as DataProperty).Reset();

		// 	_dbObject.hideFlags = HideFlags.HideInHierarchy;

		// 	AssetDatabase.AddObjectToAsset(_dbObject, _dataLibrary);

		// 	EditorUtility.SetDirty(_dbObject);
		// 	//AssetDatabase.SaveAssets();
		// 	//AssetDatabase.Refresh();


        //     string assetPath = AssetDatabase.GetAssetPath(_dbObject);
        //     var _guidString = AssetDatabase.AssetPathToGUID(assetPath);
        //     GUID _guid = GUID.Generate();
        //     GUID.TryParse(_guidString, out _guid);
        //     AssetDatabase.SaveAssetIfDirty(_guid);


        //     // DataObjectCreator.AddDataObject(_dataLibrary, _type, _dbObject as DataObject);
		// 	_dataObject.AddSubDataObject(_dbObject as DataProperty);

        //     return _dbObject as DataProperty;
		// }
    }
}
#endif
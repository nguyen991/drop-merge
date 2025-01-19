/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;

namespace Databrain.Logic.Utils
{
    [CreateAssetMenu(fileName = "MyNode", menuName = "Databrain/Logic/New Node", order = 100)]
    public class CreateNodePlaceholder : ScriptableObject { }
}
#endif
/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using UnityEngine;

namespace Databrain.Logic
{
    [Serializable]
    public class GenericExposedReference<T> where T : UnityEngine.Object
    {
        public ExposedReference<T> ExposedReference;

        public string name;

        public GenericExposedReference(string name)
        {
            this.name = name;
        }
    }
}
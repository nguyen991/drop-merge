/*
 *	DATABRAIN | Logic
 *	(c) 2024 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;
using UnityEngine.UIElements;


namespace Databrain.Logic.Manipulators
{
    public class ManipulatorManager
    {
        private readonly Dictionary<VisualElement, List<IManipulator>> elementManipulators = new Dictionary<VisualElement, List<IManipulator>>();

        public void AddManipulator(VisualElement element, IManipulator manipulator)
        {
            if (!elementManipulators.TryGetValue(element, out var manipulators))
            {
                manipulators = new List<IManipulator>();
                elementManipulators[element] = manipulators;
            }
            manipulators.Add(manipulator);
            element.AddManipulator(manipulator);
        }

        public T GetManipulator<T>(VisualElement element) where T : class, IManipulator
        {
            if (elementManipulators.TryGetValue(element, out var manipulators))
            {
                foreach (var manipulator in manipulators)
                {
                    if (manipulator is T tManipulator)
                    {
                        return tManipulator;
                    }
                }
            }
            return null;
        }
    }
}
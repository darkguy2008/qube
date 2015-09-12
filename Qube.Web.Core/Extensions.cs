using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Qube.Web.Core
{
    public static class Extensions
    {

        public static List<Control> GetControls(this Page p)
        {
            List<Control> rv = new List<Control>();
            foreach (Control c in p.Controls)
                rv.Add(c);
            return rv;
        }

        public static Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
            {
                return root;
            }
            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        // http://stackoverflow.com/questions/4955769/better-way-to-find-control-in-asp-net
        // Enhanced by DARKGuy (explicitType logic)
        /// <summary>
        /// Finds all controls of type T stores them in FoundControls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ControlFinder<T>
        {
            private readonly List<T> _foundControls = new List<T>();
            public IEnumerable<T> FoundControls
            {
                get { return _foundControls; }
            }

            public void FindChildControlsRecursive(Control control, bool explicitType = true)
            {
                foreach (Control childControl in control.Controls)
                {
                    if (explicitType)
                    {
                        if (childControl.GetType() == typeof(T))
                            _foundControls.Add((T)(object)childControl);
                        else
                            FindChildControlsRecursive(childControl, explicitType);
                    }
                    else
                    {
                        if (childControl.GetType() == typeof(T))
                            _foundControls.Add((T)(object)childControl);
                        else
                        {
                            bool add = false;
                            Type t = childControl.GetType().BaseType;
                            while (t != null)
                            {
                                add = t == typeof(T) || childControl.GetType().GetInterfaces().Contains(typeof(T));
                                if(!add)
                                    t = t.BaseType;
                                else
                                    break;
                            }
                            if(add)
                                _foundControls.Add((T)(object)childControl);
                            else
                                FindChildControlsRecursive(childControl, explicitType);
                        }
                    }
                }
            }
        }

    }
}

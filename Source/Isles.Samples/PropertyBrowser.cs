using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;


namespace Isles.Samples
{
    public partial class PropertyBrowser : UserControl
    {
        private object selected = null;

        public object SelectedObject
        {
            get { return selected; }
            set { selected = value; OnSelect(); }
        }


        public PropertyBrowser()
        {
            InitializeComponent();
        }

        private void OnSelect()
        {
            propertyGrid.SelectedObject = selected;

            if (selected == null)
            {
                treeView.Nodes.Clear();
            }
            else
            {
                TreeNode root = new TreeNode(selected.GetType().Name);
                root.Tag = selected;
                
                ReflectType(root);

                treeView.Nodes.Add(root);

                root.Expand();
            }
        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode node in e.Node.Nodes)
            {
                ReflectType(node);
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid.SelectedObject = e.Node.Tag;
        }

        private void ReflectType(TreeNode node)
        {
            object o = node.Tag;

            // IEnumerable
            if (o is ICollection && (o as ICollection).Count <= 256)
            {
                foreach (object y in o as ICollection)
                {
                    TreeNode z = new TreeNode(y.GetType().Name);
                    z.Tag = y;
                    node.Nodes.Add(z);
                }
            }

            PropertyInfo[] properties = o.GetType().GetProperties();

            foreach (PropertyInfo info in properties)
            {
                // Ignore value types
                if (info.PropertyType.IsValueType || info.PropertyType.IsEnum ||
                    info.PropertyType.IsPrimitive)
                    continue;

                MethodInfo get = info.GetGetMethod();
                
                if (get != null && !get.IsStatic)
                {
                    ParameterInfo[] parameters = info.GetIndexParameters();

                    if (parameters.Length == 0)
                    {
                        object x = info.GetValue(o, null);

                        if (x != null)
                        {
                            TreeNode n = new TreeNode(info.Name);
                            n.Tag = x;
                            node.Nodes.Add(n);
                        }
                    }
                }
            }
        }


        public static PropertyBrowser ShowForm(string title)
        {
            PropertyBrowser browser = new PropertyBrowser();
            Form form = new Form();

            form.Width = browser.Width;
            form.Height = browser.Height;

            browser.Dock = DockStyle.Fill;

            form.SuspendLayout();
            form.Text = title;
            form.Controls.Add(browser);
            form.ResumeLayout();
            form.Show();

            return browser;
        }
    }
}

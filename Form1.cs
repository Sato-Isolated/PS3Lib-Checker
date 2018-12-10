using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace Checker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static string FileName = null;
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public void GetDLL()
        {
            List<string> infecteddirectory = new List<string>();
            using (IEnumerator<string> enumerator = GetFiles(path, "PS3Lib.dll", SearchOption.AllDirectories).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FileName = enumerator.Current;
                    try
                    {
                        ModuleDefMD module = ModuleDefMD.Load(FileName);
                        if (CheckLib(module))
                        {
                            richTextBox1.SelectionColor = Color.Red;
                            richTextBox1.AppendText(FileName + Environment.NewLine);
                            infecteddirectory.Add(FileName);                           
                        }
                        else
                        {
                            richTextBox1.SelectionColor = Color.Green;
                            richTextBox1.AppendText(FileName + Environment.NewLine);
                        }
                    }
                    catch { }
                }
            }
            File.WriteAllLines(Application.StartupPath + @"\Infected.txt", infecteddirectory);
            MessageBox.Show("L'application va redémarrer");
            Application.Restart();
        }

        public static bool CheckLib(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (type.Name.Equals("PS3API"))
                    {
                        if (method.Name.Equals("MakeInstanceAPI"))
                        {
                            if (method.HasBody == false)
                            continue;
                            if (method.Body.HasInstructions)
                            {
                                for (int i = 0; i < method.Body.Instructions.Count; i++)
                                {
                                    if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr && method.Body.Instructions[i].Operand.ToString().Contains("TMAPI"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern, SearchOption p)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                string path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch { }
                if (next != null && next.Length != 0)
                {
                    foreach (string file in next)
                    {
                        yield
                        return file;
                    }
                }
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (string item in next)
                    {
                        pending.Push(item);
                    }
                }
                catch { }
            }
            yield
            break;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetDLL();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + @"\Infected.txt"))
            {
                string[] directory = File.ReadAllLines(Application.StartupPath + @"\Infected.txt");
                int i = 0;
                foreach (string line in directory)
                {
                    i++;
                    File.Delete(line);
                }
                MessageBox.Show(i.ToString() + " Dll infectées ont été supprimées !", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                File.Delete(Application.StartupPath + @"\Infected.txt");
            }
        }
    }
}
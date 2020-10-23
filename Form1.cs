using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace PS3Lib_Checker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static string _fileName;
        private readonly string _path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
   

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
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (next != null && next.Length != 0)
                {
                    foreach (var file in next)
                    {
                        yield
                        return file;
                    }
                }
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var item in next)
                    {
                        pending.Push(item);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var infecteddirectory = new List<string>();
            using (var enumerator = GetFiles(_path, "PS3Lib.dll", SearchOption.AllDirectories).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    _fileName = enumerator.Current;
                    try
                    {
                        var module = ModuleDefMD.Load(_fileName);
                        if (CheckLib(module))
                        {
                            richTextBox1.SelectionColor = Color.Red;
                            richTextBox1.AppendText(_fileName + Environment.NewLine);
                            infecteddirectory.Add(_fileName);
                        }
                        else
                        {
                            richTextBox1.SelectionColor = Color.Green;
                            richTextBox1.AppendText(_fileName + Environment.NewLine);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            File.WriteAllLines(Application.StartupPath + @"\Infected.txt", infecteddirectory);
            MessageBox.Show(@"L'application va redémarrer");
            Application.Restart();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Application.StartupPath + @"\Infected.txt")) return;
            var directory = File.ReadAllLines(Application.StartupPath + @"\Infected.txt");
            var i = 0;
            foreach (var line in directory)
            {
                i++;
                File.Delete(line);
            }
            MessageBox.Show(i + @" Dll infectées ont été supprimées !", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            File.Delete(Application.StartupPath + @"\Infected.txt");
        }
    }
}

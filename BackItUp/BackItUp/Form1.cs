using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BackItUp
{
    public partial class Form1 : Form
    {
        FolderBrowserDialog openFileDialog1 = new FolderBrowserDialog();
        static List<FileInfo> sfiles = new List<FileInfo>();  // List that will hold the files and subfiles in path
        static List<DirectoryInfo> sfolders = new List<DirectoryInfo>(); // List that hold direcotries that cannot be accessed

        static List<FileInfo> dfiles = new List<FileInfo>();  // List that will hold the files and subfiles in path
        static List<DirectoryInfo> dfolders = new List<DirectoryInfo>(); // List that hold direcotries that cannot be accessed

        static List<FileInfo> newFiles = new List<FileInfo>();

        private string SourceAdd;
        private string DestinationAdd;
        private string BackUpFolder;

        private string Source, Backup;
        private long sizeOfSource;
        private long readFileSize=0;

        private Thread threadBackup;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            //string[] files = Directory.GetFiles(openFileDialog1.SelectedPath);
            //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

            if (result.ToString() == "OK")
            {
                textBox1.Text = openFileDialog1.SelectedPath.ToString();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {

            DialogResult result = openFileDialog1.ShowDialog();

            //string[] files = Directory.GetFiles(openFileDialog1.SelectedPath);
            //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");


            if (result.ToString() == "OK" )
            {
                textBox2.Text = openFileDialog1.SelectedPath.ToString();
                
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            /*
            DirectoryInfo sdi = new DirectoryInfo(SourceAdd);
            sFullDirList(sdi, "*");
            Console.WriteLine("Done");
            Console.Read();
             */
            richTextBox1.Clear();
            threadBackup = new Thread(this.BackingUp);
            threadBackup.Start();

            button3.Enabled = false;
      
        }

        static void sFullDirList(DirectoryInfo dir, string searchPattern)
        {
            // Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try
            {
                foreach (FileInfo f in dir.GetFiles(searchPattern))
                {
                    Console.WriteLine("File {0}", f.FullName);
                    sfiles.Add(f);
                }
            }
            catch
            {
                Console.WriteLine("Directory {0}  \n could not be accessed!!!!", dir.FullName);
                return;  // We alredy got an error trying to access dir so dont try to access it again
            }

            // process each directory
            // If I have been able to see the files in the directory I should also be able 
            // to look at its directories so I dont think I should place this in a try catch block
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                sfolders.Add(d);
                sFullDirList(d, searchPattern);
            }
        }
        static void dFullDirList(DirectoryInfo dir, string searchPattern)
        {
            // Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try
            {
                foreach (FileInfo f in dir.GetFiles(searchPattern))
                {
                    Console.WriteLine("File {0}", f.FullName);
                    dfiles.Add(f);
                }
            }
            catch
            {
                Console.WriteLine("Directory {0}  \n could not be accessed!!!!", dir.FullName);
                return;  // We alredy got an error trying to access dir so dont try to access it again
            }

            // process each directory
            // If I have been able to see the files in the directory I should also be able 
            // to look at its directories so I dont think I should place this in a try catch block
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                dfolders.Add(d);
                dFullDirList(d, searchPattern);
            }
        }
        private void createBackUpFolder(string destUrl, string sorceUrl)
        {
            string[] a = sorceUrl.Split('\\');
            string backUpfolder = a[a.Count() - 1];

            if (checkFolder(destUrl, backUpfolder))
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    richTextBox1.AppendText("Folder exist " + '\n');
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    richTextBox1.AppendText("Folder does not exist" + '\n');
                });
                try
                {
                    DirectoryInfo di = Directory.CreateDirectory(destUrl + '\\' + backUpfolder);
                    this.Invoke((MethodInvoker)delegate()
                    {
                        richTextBox1.AppendText(" Create directory successfully" + '\n');
                    });
                }
                catch (Exception e)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        richTextBox1.AppendText("Folder does not create successfully " + '\n');
                    });
                }
                

            }
        }
        private bool checkFolder(string url, string folderName)
        {
            DirectoryInfo dinfo = new DirectoryInfo(url); // Populates field with all Sub Folders
            DirectoryInfo[] directorys = dinfo.GetDirectories();
            foreach (DirectoryInfo directory in directorys)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    richTextBox1.AppendText(directory.Name + '\n');
                }); 
                if (folderName == directory.Name) return true;
            }
            return false;
        }

        private void BackUpFiles(DirectoryInfo SourceAddRef, DirectoryInfo DestinationAddRef)
        {
            foreach (FileInfo f in SourceAddRef.GetFiles("*"))
            {
                //Console.WriteLine("File {0}", f.Name);
                //sfiles.Add(f);
                readFileSize = readFileSize + f.Length;
                this.Invoke((MethodInvoker)delegate()
                {
                    progressBar2.Value = (int)(readFileSize * 100 / sizeOfSource);
                });

                if(progressBar2.Value==100)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        button3.Enabled = true;
                    }); 
                }

                int count = 0;
                foreach (FileInfo j in DestinationAddRef.GetFiles("*"))
                {
                    count++;
                }
                foreach (FileInfo j in DestinationAddRef.GetFiles("*"))
                {
                    //Console.WriteLine("File {0}", f.Name);
                    //sfiles.Add(f);
                    if (f.Name == j.Name)
                    {
                        if (f.LastWriteTime != j.LastWriteTime)
                        {
                            File.Copy(SourceAddRef.FullName + "\\" + f.Name, DestinationAddRef.FullName + "\\" + f.Name, true);
                            this.Invoke((MethodInvoker)delegate()
                            {
                                richTextBox1.AppendText("Overwrite file : " + DestinationAddRef.FullName + "\\" + f.Name + '\n');
                            });
                            
                        }
                        break;
                    }
                    count--;
                }
                if (count == 0)
                {
                    //file does not in the folder so copy the file;
                    File.Copy(SourceAddRef.FullName + "\\" + f.Name, DestinationAddRef.FullName + "\\" + f.Name);
                    this.Invoke((MethodInvoker)delegate()
                    {
                        richTextBox1.AppendText("Coppied file : " + DestinationAddRef.FullName + "\\" + f.Name + '\n');
                    });
                }
            } 
        }
        private void BackUpFolders(DirectoryInfo SourceAddRef, DirectoryInfo DestinationAddRef)
        {
            foreach (DirectoryInfo f in SourceAddRef.GetDirectories())
            {
                //Console.WriteLine("File {0}", f.Name);
                //sfiles.Add(f);
                int count = 0;
                foreach (DirectoryInfo j in DestinationAddRef.GetDirectories())
                {
                    count++;
                }
                foreach (DirectoryInfo j in DestinationAddRef.GetDirectories())
                {
                    //Console.WriteLine("File {0}", f.Name);
                    //sfiles.Add(f);
                    if (f.Name == j.Name)
                    {
                        BackUpFolders(new DirectoryInfo(SourceAddRef.FullName + "\\" + f.Name), new DirectoryInfo(DestinationAddRef.FullName + "\\" + f.Name));
                        break;
                    }
                    count--;
                }
                if (count == 0)
                {
                    //folder does not in the folder so create a folder;
                    Directory.CreateDirectory(DestinationAddRef.FullName + "\\" + f.Name);
                    this.Invoke((MethodInvoker)delegate()
                    {
                        richTextBox1.AppendText("Directory Created : " + DestinationAddRef.FullName + "\\" + f.Name + '\n');
                    });
                    BackUpFolders(new DirectoryInfo(SourceAddRef.FullName + "\\" + f.Name), new DirectoryInfo(DestinationAddRef.FullName + "\\" + f.Name));
                }
            }            
            BackUpFiles( SourceAddRef, DestinationAddRef); 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string[] a = textBox1.Text.Split('\\');
            textBox3.Text = BackUpFolder = a[a.Count() - 1];
            SourceAdd = textBox1.Text;
            Source = SourceAdd;

            DestinationAdd = textBox2.Text;
            string tmp = DestinationAdd + "\\" + BackUpFolder;
            Backup = tmp;

            try
            {
                DirectoryInfo h = new DirectoryInfo(textBox1.Text);
                DirectoryInfo l = new DirectoryInfo(textBox2.Text);

                sizeOfSource = GetDirectorySize(textBox1.Text);
                //MessageBox.Show(dirSize.ToString());

                button3.Enabled = true;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button5.Visible = true;
                button4.Visible = false;

                textBox4.Text = (sizeOfSource/1024).ToString();
            }
            catch( Exception eb) 
            {
                MessageBox.Show("Your Source or Destination is not valid");
                button3.Enabled = false;
            }

        }
        private void button5_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button5.Visible = false;
            button4.Visible = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            button3.Enabled = false;
        }
        private static long GetDirectorySize(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] subdirectories = Directory.GetDirectories(path);

            long size = files.Sum(x => new FileInfo(x).Length);
            foreach (string s in subdirectories)
                size += GetDirectorySize(s);

            return size;
        }
        private void progressBar2_Click(object sender, EventArgs e)
        {

        }

        private void BackingUp()
        {
            readFileSize = 0;
            this.Invoke((MethodInvoker)delegate()
            {
                progressBar2.Value = 0;
            });
            createBackUpFolder(DestinationAdd, SourceAdd);
            BackUpFolders(new DirectoryInfo(Source), new DirectoryInfo(Backup));
            this.Invoke((MethodInvoker)delegate()
            {
                MessageBox.Show("Backup complete");
            });
            this.threadBackup.Abort();
        }
    }
}

/*
 static void Main(string[] args)
{
    DirectoryInfo di = new DirectoryInfo("A:\\");
    FullDirList(di, "*");
    Console.WriteLine("Done");
    Console.Read();
}



}
 */
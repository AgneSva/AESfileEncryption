using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
 
namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //gija uzkoduoti kataloga:
            Thread th = new Thread(() => { koduok(); });
            th.Start();

        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;


            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }
        private void koduok()
        {
            _ = this.Invoke((Action)delegate
            {

                //paspaudziam"koduot" ir ieskom ka uzkoduot
                FolderBrowserDialog FBD = new FolderBrowserDialog();

                if (FBD.ShowDialog() == DialogResult.OK)
                {   //visi failai kataloge:
                    string[] fileArray = Directory.GetFiles(FBD.SelectedPath);

                    string password = "abcd1234";
                    //Array.ForEach(fileArray, Console.WriteLine);

                    //for progress bar--------
                    int k = 0;
                    //is viso rezultatu:
                    var fileCount = fileArray.Count();
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = fileCount;

                    //isvalyti faila su md5 reiksmem
                    System.IO.File.WriteAllText("md5.txt", string.Empty);

                    //for every file in the fileArray do this:
                    foreach (string file in fileArray)
                    {
                       
                        ////if its a file for md5 values- skip
                        if (Path.GetFileName(file) == "md5.txt")
                        {

                            continue;
                        }


                        byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
                        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                        // Hash the password with SHA256
                        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                        byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

                        //string fileEncrypted = "myfile.txt";

                        File.WriteAllBytes(file, bytesEncrypted);

                        //apskaiciuojam md5 hash reiksme
                        string reiksme = CalculateMD5(file);
                        //Console.WriteLine(reiksme);
                        //isaugom md5 reiksme faile
                       //File.AppendAllText("md5.txt", reiksme);
                        File.AppendAllText("md5.txt",reiksme + Environment.NewLine);

                        //Progress bar'o pildymas
                        while (k <= fileCount)
                        {
                            progressBar1.Value = k;
                            k++;
                        }


                    }

                }

            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //gija atkoduoti kataloga:
            Thread th1 = new Thread(() => { atkoduok(); });
            th1.Start();
        }


        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;


            // The salt bytes > 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        private void atkoduok()
        {
            _ = this.Invoke((Action)delegate
            {
                //atkoduoja katalogo failus-
                FolderBrowserDialog FBD = new FolderBrowserDialog();

                if (FBD.ShowDialog() == DialogResult.OK)
                {
                    //visi failai kataloge:
                    string[] fileArray = Directory.GetFiles(FBD.SelectedPath);

                    string password = "abcd1234";
                    // Array.ForEach(fileArray, Console.WriteLine);

                    //for progress bar--------
                    int k = 0;
                    //is viso rezultatu:
                    var fileCount = fileArray.Count();
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = fileCount;


                    //for every file in the fileArray do this:
                    foreach (string fileEncrypted in fileArray)
                    {


                        if (Path.GetFileName(fileEncrypted) == "md5.txt")
                        {

                            continue;
                        }
                        //apskaiciuojam md5 hash reiksme pries atkoduojant
                        string reiksme = CalculateMD5(fileEncrypted);
                        Console.WriteLine(reiksme);


                        //if tokia reiksme neegzistuoja md5.txt - neleisti atkoduoti
                        var lines = File.ReadAllLines("md5.txt");

                        if (!lines.Contains(reiksme))
                        {
                            Console.WriteLine("neeigzistuoja toks md5");
                            break;
                        }




                        byte[] bytesToBeDecrypted = File.ReadAllBytes(fileEncrypted);
                        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                        byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);


                        File.WriteAllBytes(fileEncrypted, bytesDecrypted);

                        //Progress bar'o pildymas
                        while (k <= fileCount)
                        {
                            progressBar1.Value = k;
                            k++;
                        }


                    }
                }
            });
        }

    }
}

using AForge.Imaging;
using OCRAPITest.Google;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);




        //Mouse actions
        public const int MOUSEEVENTF_LEFTDOWN = 0x201;
        public const int MOUSEEVENTF_LEFTUP = 0x202;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x204;
        public const int MOUSEEVENTF_RIGHTUP = 0x205;

        public int x = 0;
        public int y = 0;
        public string monsterName = "";
        Monster m = new Monster();

        public Form1()
        {

            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            CreateMonsters();
            Bitmap battleIconH = (Bitmap)Bitmap.FromFile(@"C:\Users\Biga\Desktop\battle-master\BattleMode\BattleMode\img\BATTLEICONH.jpg");
            Bitmap battleIconL = (Bitmap)Bitmap.FromFile(@"C:\Users\Biga\Desktop\battle-master\BattleMode\BattleMode\img\BATTLEICONL.jpg");
            Bitmap battleIconLowOpen = (Bitmap)Bitmap.FromFile(@"C:\Users\Biga\Desktop\battle-master\BattleMode\BattleMode\img\battleNotOpenedLowResolution.jpg");

            Process cointainsProcess = Process.GetProcessesByName("client").FirstOrDefault();

            while (cointainsProcess != null)
            {
                IntPtr h = cointainsProcess.MainWindowHandle;
                printImageScreen();

                //verify if battle high resolution is opened
                if (verifyImage(ConvertToFormat(printImageScreen(), battleIconH.PixelFormat), battleIconH))
                {
                    attackProcess(cointainsProcess);
                }
                //verify if battle low resolution is opened
                else if (verifyImage(ConvertToFormat(printImageScreen(), battleIconL.PixelFormat), battleIconL))
                {
                    attackProcess(cointainsProcess);
                }
                //open battle
                else if(verifyImage(ConvertToFormat(printImageScreen(), battleIconLowOpen.PixelFormat), battleIconLowOpen))
                {
                    Point p = new Point(x - 10, y + 10);
                    int position = ((p.Y << 0x10) | (p.X & 0xFFFF));
                    IntPtr handle = FindWindow(null, cointainsProcess.MainWindowTitle);
                    // Send the click message    
                    PostMessage(handle, MOUSEEVENTF_LEFTDOWN, new IntPtr(0x01), new IntPtr(position));
                    PostMessage(handle, MOUSEEVENTF_LEFTUP, new IntPtr(0), new IntPtr(position));
                    attackProcess(cointainsProcess);
                }
            }
        }

        public void attackProcess(Process cointainsProcess)
        {
        
            do
            {
                if (haveMonster())
                {
                    var imageToGetText = getBattlePrint();

                    RecognizeGoogleApi(imageToGetText);
                    
                    if (m.checkMonster(monsterName))
                    {
                        //if determinado pixel dentro do battle estiver vermelho, não faz nada abaixo
                        if (!isAttacking())
                        {                
                            Point p = new Point(x + 87, y + 25);                

                            int position = ((p.Y << 0x10) | (p.X & 0xFFFF));

                            IntPtr handle = FindWindow(null, cointainsProcess.MainWindowTitle);

                            // Send the click message                      

                            PostMessage(handle, MOUSEEVENTF_LEFTDOWN, new IntPtr(0x01), new IntPtr(position));
                            PostMessage(handle, MOUSEEVENTF_LEFTUP, new IntPtr(0), new IntPtr(position));
                        }
                    }
                }
            } while (true);
        }

        public async void RecognizeGoogleApi(Bitmap imageToGetText)
        {
            Annotate annotate = new Annotate();
            Application.DoEvents();
            try
            {
                await annotate.GetText(imageToGetText, "en", "TEXT_DETECTION");
                if (string.IsNullOrEmpty(annotate.Error) == false)
                    MessageBox.Show("ERROR: " + annotate.Error);
                else
                    monsterName = annotate.TextResult;
            }
            catch
            {

            }
        }

        private bool haveMonster(){           
            Bitmap verifyBattle = printImageScreen();
            if (verifyBattle.GetPixel(x + 10, y + 60).R.Equals(73) && verifyBattle.GetPixel(x + 10, y + 60).G.Equals(73) && verifyBattle.GetPixel(x + 10, y + 60).B.Equals(73))
                return false;
            return true;
        }

        private Bitmap getBattlePrint()
        {
            var pos = findBattleBorder();
            Rectangle rectCropArea = new Rectangle(x, y + 10, pos.X, pos.Y);
            if (new prjTools.ScreenShot().GetWindowPictureFromRectangle(rectCropArea).GetPixel(156, 16).R == 53){
                //tem rolagem
            }
            new prjTools.ScreenShot().GetWindowPictureFromRectangle(rectCropArea).Save(@"C:\Users\Biga\Desktop\testretangle.jpg", ImageFormat.Jpeg);//tirar dps
            return new prjTools.ScreenShot().GetWindowPictureFromRectangle(rectCropArea);
        }
        private Point findBattleBorder()
        {
            Bitmap image = printImageScreen();
            try
            {
                for (int j = y; j < image.Height; j++)
                {
                    if (image.GetPixel(x + 12, j).R.Equals(118) && image.GetPixel(x + 12, j).G.Equals(118) && image.GetPixel(x + 12, j).B.Equals(118))
                    {
                        int contador = 0;
                        for (int i = x; i < image.Width; i++)
                        {
                            if (image.GetPixel(x + 12, j).R.Equals(118) && image.GetPixel(x + 12, j).G.Equals(118) && image.GetPixel(x + 12, j).B.Equals(118))
                            {
                                contador++;
                            }
                            if (contador > 160)
                            {

                                int width = i - x;
                                int height = j - y;
                                Point p = new Point(width, height);
                                return p;
                            }
                        }

                    }
                }
            }
            catch
            {

            }
           
            Point p1 = new Point(0, 0);
            return p1;
        }
        private bool isAttacking()
        {
            Rectangle crop = new Rectangle(x, y, 100, 200);

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(printImageScreen(), new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }

            if (bmp.GetPixel(24, 73).R.Equals(237) && bmp.GetPixel(24, 73).G.Equals(8) && bmp.GetPixel(24, 73).B.Equals(8))
                return true;
            return false;
        }
        private void CreateMonsters()
        {
            m.addMonster("Rat");
            m.addMonster("Cave Rat");
        }
        private Bitmap printImageScreen()
        {
            //tirar dps
            new prjTools.ScreenShot().GetWindowPicture().Save(@"C:\Users\Biga\Desktop\ovo.jpg", ImageFormat.Jpeg);
            return new prjTools.ScreenShot().GetWindowPicture();
        }
        private Bitmap ConvertToFormat(System.Drawing.Image image, PixelFormat format)
        {
            Bitmap copy = new Bitmap(image.Width, image.Height, format);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }
        private bool verifyImage(Bitmap sourceImage, Bitmap template){            
            
            // create template matching algorithm's instance
            // (set similarity threshold to 92.1%)

            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.921f);
            // find all matchings with specified above similarity

            TemplateMatch[] matchings = tm.ProcessImage(sourceImage, template);
            // highlight found matchings

            if(matchings.Length == 0)
                return false;   
            
            BitmapData data = sourceImage.LockBits(
            new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
            ImageLockMode.ReadWrite, sourceImage.PixelFormat);
            foreach (TemplateMatch m in matchings)
            {
                Drawing.Rectangle(data, m.Rectangle, System.Drawing.Color.White);
                x = m.Rectangle.Location.X;
                y = m.Rectangle.Location.Y ;
            }
            // do something else with matching
            sourceImage.UnlockBits(data);

            return true;
        }
    }
}

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
      
        public int scrollBattlePosX = 0;
        public int scrollBattlePosY = 0;
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
                if (verifyImage(ConvertToFormat(printImageScreen(), battleIconH.PixelFormat), battleIconH, ""))
                {
                    attackProcess(cointainsProcess);
                }
                //verify if battle low resolution is opened
                else if (verifyImage(ConvertToFormat(printImageScreen(), battleIconL.PixelFormat), battleIconL, ""))
                {
                    attackProcess(cointainsProcess);
                }
                //open battle
                else if (verifyImage(ConvertToFormat(printImageScreen(), battleIconLowOpen.PixelFormat), battleIconLowOpen, ""))
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
            int countAttackTime = 0;
            do
            {

                if (haveMonster())
                {
                    var imageToGetText = getBattlePrint();
                    if (!scrollBattleExists(imageToGetText))
                    {
                        if (countAttackTime > 0)
                        {
                            countAttackTime = 0;
                            break;
                        }                       
                        RecognizeGoogleApi(imageToGetText);
                        var monsterPosition = m.checkMonster(monsterName);
                        if (!isAttacking())
                        {
                            if (monsterPosition != 0)
                            {
                                int newYPos = y + (monsterPosition * 21);
                                countAttackTime++;

                                Point p = new Point(x + 87, newYPos);
                                int position = ((p.Y << 0x10) | (p.X & 0xFFFF));


                                IntPtr handle = FindWindow(null, cointainsProcess.MainWindowTitle);

                                // Send the click message                      

                                PostMessage(handle, MOUSEEVENTF_LEFTDOWN, new IntPtr(0x01), new IntPtr(position));
                                PostMessage(handle, MOUSEEVENTF_LEFTUP, new IntPtr(0), new IntPtr(position));
                            }
                        }
                    }
                    else
                    {
                        if (countAttackTime > 0)
                        {
                            countAttackTime = 0;
                            break;
                        }
                        RecognizeGoogleApi(imageToGetText);
                        var monsterPosition = m.checkMonster(monsterName);
                        if (!isAttacking())
                        {
                            if (monsterPosition != 0)
                            {
                                countAttackTime++;
                                int newYPos = y + (monsterPosition * 17); 
                                Point p = new Point(x + 87, newYPos);
                                int position = ((p.Y << 0x10) | (p.X & 0xFFFF));
                                IntPtr handle = FindWindow(null, cointainsProcess.MainWindowTitle);
                                PostMessage(handle, MOUSEEVENTF_LEFTDOWN, new IntPtr(0x01), new IntPtr(position));
                                PostMessage(handle, MOUSEEVENTF_LEFTUP, new IntPtr(0), new IntPtr(position));
                            }
                            else
                            {
                                Point p = new Point(x + scrollBattlePosX + 15, y + scrollBattlePosY + 15);

                                int position = ((p.Y << 0x10) | (p.X & 0xFFFF));

                                IntPtr handle = FindWindow(null, cointainsProcess.MainWindowTitle);

                                PostMessage(handle, MOUSEEVENTF_LEFTDOWN, new IntPtr(0x01), new IntPtr(position));
                                PostMessage(handle, MOUSEEVENTF_LEFTUP, new IntPtr(0), new IntPtr(position));
                            }
                           
                        }
                    }
                }
            } while (true);
        }

        private bool scrollBattleExists(Bitmap battleRetangle)
        {
            var pos = findBattleBorder();
            Bitmap scrollBattlelDL = (Bitmap)Bitmap.FromFile(@"C:\Users\Biga\Desktop\battle-master\BattleMode\BattleMode\img\scrollBattleImageDownLowResolution.jpg");
            Bitmap scrollBattleUL = (Bitmap)Bitmap.FromFile(@"C:\Users\Biga\Desktop\battle-master\BattleMode\BattleMode\img\scrollBattleImageUpLowResolution.jpg");
            Rectangle rectCropArea = new Rectangle(x + 10, y + 10, pos.X, pos.Y);
                if (new prjTools.ScreenShot().GetWindowPictureFromRectangle(rectCropArea).GetPixel(146, 16).R == 53)
                {
                    return false;
                }
                else if (verifyImage(ConvertToFormat(battleRetangle, scrollBattleUL.PixelFormat), scrollBattleUL, "FindScroll"))
                {
                    return true;
                }
            
           
          
            return true;
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
        private bool haveMonster()
        {
            Bitmap verifyBattle = getBattlePrint();
            int count = 0;
            for(int j = 0; j< verifyBattle.Height; j++)
            {
                if (verifyBattle.GetPixel(13, j).R.Equals(74) && verifyBattle.GetPixel(13, j).G.Equals(74) && verifyBattle.GetPixel(13, j).B.Equals(74))
                    count++;
            }
            if (count > 30)
                return false;
            
           
       
            return true;
        }
        private Bitmap getBattlePrint()
        {
            var pos = findBattleBorder();
            Rectangle rectCropArea = new Rectangle(x+10, y + 10, pos.X, pos.Y);
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
            var print = getBattlePrint();
            

            for (int j = 0; j < print.Height; j++)
            {
                var x1 = print.GetPixel(8, j).R;
                var x2 = print.GetPixel(8, j).G;
                var x3 = print.GetPixel(8, j).B;
                if (print.GetPixel(8, j).R.Equals(255) && print.GetPixel(8, j).G.Equals(0) && print.GetPixel(8, j).B.Equals(0))
                    return true;
            }
            return false;
        }
        private void CreateMonsters()
        {
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
        private bool verifyImage(Bitmap sourceImage, Bitmap template, string typeFormat)
        {

            // create template matching algorithm's instance
            // (set similarity threshold to 92.1%)

            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.921f);
            // find all matchings with specified above similarity

            TemplateMatch[] matchings = tm.ProcessImage(sourceImage, template);
            // highlight found matchings

            if (matchings.Length == 0)
                return false;

            BitmapData data = sourceImage.LockBits(
            new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
            ImageLockMode.ReadWrite, sourceImage.PixelFormat);
            foreach (TemplateMatch m in matchings)
            {
                Drawing.Rectangle(data, m.Rectangle, System.Drawing.Color.White);
                if (typeFormat == "FindScroll")
                {
                    scrollBattlePosX = m.Rectangle.Location.X;
                    scrollBattlePosY = m.Rectangle.Location.Y;

                }
                else
                {
                     x = m.Rectangle.Location.X;
                     y = m.Rectangle.Location.Y;
                }
            
                               
            }
            // do something else with matching
            sourceImage.UnlockBits(data);

            return true;
        }
    }
}
   


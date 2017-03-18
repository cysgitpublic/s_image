using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using System.Drawing.Imaging;

using System.Threading;
namespace image
{
    public partial class Form1 : Form
    {

        protected int lastX = 0;
        protected int lastY = 0;
        protected string lastFilename = String.Empty;

        protected DragDropEffects effect;
        protected bool validData;
        protected Image image;
        protected Image nextImage;
        protected Thread getImageThread;

        List<PictureBox> lstPictureBox = new List<PictureBox>();
        public Form1()
        {
            InitializeComponent();

            lstPictureBox.Add(pictureBox1);
            lstPictureBox.Add(pictureBox2);
            lstPictureBox.Add(pictureBox3);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String win1 = "Test Window"; //The name of the window
            CvInvoke.NamedWindow(win1); //Create the window using the specific name

            Mat img = new Mat(200, 400, DepthType.Cv8U, 3); //Create a 3 channel image of 400x200
            img.SetTo(new Bgr(255, 0, 0).MCvScalar); // set it to Blue color

            //Draw "Hello, world." on the image using the specific font
            CvInvoke.PutText(
               img,
               "Hello, world",
               new System.Drawing.Point(10, 80),
               FontFace.HersheyComplex,
               1.0,
               new Bgr(0, 255, 0).MCvScalar);


            CvInvoke.Imshow(win1, img); //Show the image
            CvInvoke.WaitKey(0);  //Wait for the key pressing event
            CvInvoke.DestroyWindow(win1); //Destroy the window if key is pressed
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgra, Byte> My_Image = new Image<Bgra, byte>(Openfile.FileName);
                double x = 20;
                My_Image = My_Image.Rotate(x, new Emgu.CV.Structure.Bgra(255, 255, 255, 0), false);
                pictureBox1.Image = My_Image.ToBitmap();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    FormLayout form = new FormLayout();
                    form.SetFolder(folderDialog.SelectedPath);

                    form.RenderImage();
                    form.Show();

                    Bitmap bmp = new Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));

                    string fn = Path.Combine(folderDialog.SelectedPath, "screen.png");
                    bmp.Save(fn, ImageFormat.Png);

                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            int x = this.PointToClient(new Point(e.X, e.Y)).X;
            int y = this.PointToClient(new Point(e.X, e.Y)).Y;
            foreach (PictureBox pictureBox in lstPictureBox)
            {
                if (x >= pictureBox.Location.X && x <= pictureBox.Location.X + pictureBox.Width && y >= pictureBox.Location.Y && y <= pictureBox.Location.Y + pictureBox.Height)
                {
                    string filename;
                    validData = GetFilename(out filename, e);
                    if (validData)
                    {
                        Image<Bgra, Byte> image = new Image<Bgra, byte>(filename);
                        if (pictureBox.Image != null)
                        {
                            pictureBox.Image.Dispose();
                        }
                        image = image.Resize(pictureBox.Width, pictureBox.Height, Inter.Linear, false);
                        pictureBox.Image = image.ToBitmap();
                    }
                }
            }
            thumbnail.Visible = false;
        }
        public delegate void AssignImageDlgt();
        protected void LoadImage()
        {
            nextImage = new Bitmap(lastFilename);
            this.Invoke(new AssignImageDlgt(AssignImage));
        }

        protected void AssignImage()
        {
            thumbnail.Width = 100;
            // 100    iWidth
            // ---- = ------
            // tHeight  iHeight
            thumbnail.Height = nextImage.Height * 100 / nextImage.Width;
            SetThumbnailLocation(this.PointToClient(new Point(lastX, lastY)));
            thumbnail.Image = nextImage;
        }

        protected void SetThumbnailLocation(Point p)
        {
            if (thumbnail.Image == null)
            {
                thumbnail.Visible = false;
            }
            else
            {
                p.X -= thumbnail.Width / 2;
                p.Y -= thumbnail.Height / 2;
                thumbnail.Location = p;
                thumbnail.Visible = true;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            validData = GetFilename(out filename, e);
            if (validData)
            {
                if (lastFilename != filename)
                {
                    thumbnail.Image = null;
                    thumbnail.Visible = false;
                    lastFilename = filename;
                    getImageThread = new Thread(new ThreadStart(LoadImage));
                    getImageThread.Start();
                }
                else
                {
                    thumbnail.Visible = true;
                }
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            thumbnail.Visible = false;
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (validData)
            {
                if ((e.X != lastX) || (e.Y != lastY))
                {
                    SetThumbnailLocation(this.PointToClient(new Point(e.X, e.Y)));
                }
            }
        }

        protected bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp"))
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            /*
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {

                Image<Bgra, Byte> My_Image = new Image<Bgra, byte>(Openfile.FileName);
                My_Image = My_Image.Rotate(x, new Emgu.CV.Structure.Bgra(255, 255, 255, 0), false);
                pictureBox1.Image = My_Image.ToBitmap();
            }
            */
            SelectFile(sender, e);
        }

        private void SelectFile(object box, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                SetImage((PictureBox)box, Openfile.FileName);
            }
        }

        private void SetImage(PictureBox box, String fileName)
        {
            Image<Bgra, Byte> image = new Image<Bgra, byte>(fileName);
            //My_Image = My_Image.Rotate(x, new Emgu.CV.Structure.Bgra(255, 255, 255, 0), false);
            if (box.Image != null)
            {
                box.Image.Dispose();
            }
            image = image.Resize(box.Width, box.Height, Inter.Linear, false);

            box.Image = image.ToBitmap();
        }
    }
}

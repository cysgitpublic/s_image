using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace image
{
    public partial class FormLayout : Form
    {
        enum enum_effect
        {
            ENUM_NONE,
            ENUM_ROTATE,
            ENUM_FLIP
        };

        String m_strFolder;
        List<PictureBox> lstPictureBox = new List<PictureBox>();
        Dictionary<PictureBox, enum_effect> dicEffect = new Dictionary<PictureBox, enum_effect>();
        List<String> lstFileName = new List<String>();

        
        public FormLayout()
        {
            InitializeComponent();

            lstPictureBox.Add(pictureBox1);
            lstPictureBox.Add(pictureBox2);
            lstPictureBox.Add(pictureBox3);
            dicEffect[pictureBox3] = enum_effect.ENUM_FLIP;
            lstPictureBox.Add(pictureBox4);
            dicEffect[pictureBox4] = enum_effect.ENUM_ROTATE;
        }

        public void SetFolder(String strFolder)
        {
            m_strFolder = strFolder;

            string[] filePaths = Directory.GetFiles(strFolder, "*.jpg");
            foreach(string filePath in filePaths)
            {
                lstFileName.Add(filePath);
            }
        }

        public void RenderImage()
        {
            int i = 0;
            foreach(PictureBox box in lstPictureBox)
            {
                if(i < lstFileName.Count)
                {
                    Image<Bgra, Byte> image = new Image<Bgra, byte>(lstFileName[i]);
                    //double x = 20;
                    //My_Image = My_Image.Rotate(x, new Emgu.CV.Structure.Bgra(0, 0, 0, 0), false);
                    

                    enum_effect effect = enum_effect.ENUM_NONE;
                    if(dicEffect.TryGetValue(box, out effect))
                    {
                        if(effect == enum_effect.ENUM_ROTATE)
                        {
                            double x = 20;
                            image = image.Rotate(x, new Emgu.CV.Structure.Bgra(255, 255, 255, 0), true);
                        }
                        else if(effect == enum_effect.ENUM_FLIP)
                        {
                            image = image.Flip(FlipType.Horizontal);
                        }
                    }

                    image = image.Resize(box.Width, box.Height, Inter.Linear, true);
                    box.Image = image.ToBitmap();
                    i++;
                }
            }
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            int a = 0;
        }

        private void FormLayout_Load(object sender, EventArgs e)
        {
            ((Control)pictureBox1).AllowDrop = true;
        }
    }
}

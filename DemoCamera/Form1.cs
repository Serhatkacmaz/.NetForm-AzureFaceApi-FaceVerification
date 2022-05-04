using AForge.Video;
using AForge.Video.DirectShow;
using DemoCamera.Helpers;
using DemoCamera.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoCamera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Variables

        VideoCaptureDevice videoCapture;
        FilterInfoCollection filterInfo;
        int counterTime = 0;

        #endregion

        #region Methods

        void StartCamera()
        {
            try
            {
                filterInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                videoCapture = new VideoCaptureDevice(filterInfo[0].MonikerString);
                videoCapture.NewFrame += new NewFrameEventHandler(Camera_On);
                videoCapture.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        void CapturePhotoAndSave()
        {
            try
            {
                string filePath = Environment.CurrentDirectory + ".jpg"; //->Debug.jpg

                var bitmap = new Bitmap(pic1.Width, pic1.Height);
                pic1.DrawToBitmap(bitmap, pic1.ClientRectangle);

                ImageFormat imageFormat = null;
                imageFormat = ImageFormat.Jpeg;
                bitmap.Save(filePath, imageFormat);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Hata Oluştu Uygulamayı Yeniden Başlat\n\n" + exception.Message);
                Application.Exit();
            }
            
        }

        #endregion

        #region Events
        private void Form1_Load(object sender, EventArgs e)
        {
            StartCamera();
            txtInfo.Text = "Kamera Açık";
        }

        private void Camera_On(object sender, NewFrameEventArgs eventArgs)
        {
            pic1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                videoCapture.Stop();
            }
            catch (Exception)
            {
                return;
            }
        }

        private async void btnControl_Click(object sender, EventArgs e)
        {
            CapturePhotoAndSave();

            try
            {
                //-> Azure FaceAPI
                string filePath = Environment.CurrentDirectory + ".jpg";
                var face = await FaceAPI.MakeAnalysisRequest(filePath);
                var faceModel = FaceModel.FromJson(face);
                var faceID = faceModel.First().FaceId;
                var response = await FaceAPI.Similarity(faceID);
                var attributes = faceModel.First().FaceAttributes;
                string gender = attributes.Gender.ToString() == "male" ? "Erkek" : "Kadın";
                lblInfo.Text = "Tahmini Yaş -> " + attributes.Age.ToString() + " Cinsiyet -> " + gender.ToString();

                if (response)
                {
                    btnControl.Enabled = false;
                    txtInfo.Text = "Kapı Açılıyor";
                    pic2.Image = DemoCamera.Properties.Resources.open;
                    timer1.Start();
                }
                else
                {
                    txtInfo.Text = "Kapı Açılmaz, Tanımsız Kişi";
                    pic2.Image = DemoCamera.Properties.Resources.close;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Hata Oluştu Uygulamayı Yeniden Başlat\n\n" + exception.Message);
                Application.Exit();
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            counterTime++;
            int endTimer = 10;

            if (counterTime == endTimer)
            {
                counterTime = 0;
                txtInfo.Text = "Kapı Kapandı";
                pic2.Image = DemoCamera.Properties.Resources.close;
                btnControl.Enabled = true;
                lblInfo.Text = string.Empty;
                timer1.Stop();
            }
        }

        #endregion
    }
}

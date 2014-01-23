﻿using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using libface;

namespace train
{
    public partial class TrainForm : Form
    {
        /// <summary>
        /// image
        /// </summary>
        public Image<Gray, byte> FaceImage { get; set; }

        /// <summary>
        /// faceRecognizer
        /// </summary>
        private FaceRecognizer faceRecognizer;

        /// <summary>
        /// TrainForm
        /// </summary>
        public TrainForm(FaceRecognizer faceRecognizer)
        {
            InitializeComponent();

            this.faceRecognizer = faceRecognizer;
        }

        /// <summary>
        /// btnOk_Click
        /// </summary>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            string name = txtName.Text;
            faceRecognizer.addFace(name, FaceImage);
            this.Close();
            this.Dispose();
        }

        /// <summary>
        /// btnSkip_Click
        /// </summary>
        private void btnSkip_Click(object sender, System.EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}
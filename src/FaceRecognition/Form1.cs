﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using libface;

namespace train
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// faceRecognizer
        /// </summary>
        private FaceRecognizer faceRecognizer = new FaceRecognizer();

        /// <summary>
        /// Form1
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// btnLoadTrainImage_Click
        /// </summary>
        private void btnLoadTrainImage_Click(object sender, EventArgs e)
        {
            // open dialog
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() != DialogResult.OK) return;

            // set the image
            faceRecognizer.CurrentImage = new Image<Bgr, byte>(dlg.FileName);

            // recognize faces
            int foundFaces = faceRecognizer.recognizeFaces();

            // set the image
            this.imgTrain.Image = faceRecognizer.CurrentImage;

            // display number of faces found
            this.lblFoundFaces.Text = foundFaces.ToString();
        }

        /// <summary>
        /// btnTrain_Click
        /// </summary>
        private void btnTrain_Click(object sender, EventArgs e)
        {
            // get detected faces
            List<Image<Gray, Byte>> faces = faceRecognizer.getTrainFaces();

            // show dialog for each face
            foreach (Image<Gray, Byte> face in faces)
            {
                TrainForm trainForm = new TrainForm(faceRecognizer);
                trainForm.FaceImage = face;
                trainForm.ShowDialog();
            }

            // recognize faces again
            faceRecognizer.recognizeFaces();

            // set the image
            this.imgTrain.Image = faceRecognizer.CurrentImage;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
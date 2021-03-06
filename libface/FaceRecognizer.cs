﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace libface
{
    public class FaceRecognizer
    {
        #region public propeties

        /// <summary>
        /// currentImage
        /// </summary>
        public Image<Bgr, Byte> CurrentImage
        {
            get
            {
                return this._currentImage;
            }
            set
            {
                int width = value.Width;
                double scale = (double)500 / width;
                this._currentImage = value.Resize(scale, INTER.CV_INTER_CUBIC);
                this._originalImage = value.Resize(scale, INTER.CV_INTER_CUBIC);
            }
        }

        /// <summary>
        /// _currentImage
        /// </summary>
        private Image<Bgr, Byte> _currentImage;

        /// <summary>
        /// _originalImage
        /// </summary>
        private Image<Bgr, Byte> _originalImage;

        #endregion public propeties

        #region private members

        /// <summary>
        /// numTrainImages
        /// </summary>
        private int numTrainImages = 0;

        /// <summary>
        /// face haarcascade
        /// </summary>
        private HaarCascade faceHaar = new HaarCascade("haarcascade_frontalface_default.xml");

        /// <summary>
        /// font used for drawing names
        /// </summary>
        private MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_DUPLEX, 0.7d, 0.7d);

        /// <summary>
        /// trained images
        /// </summary>
        private List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();

        /// <summary>
        /// trainingNames
        /// </summary>
        private List<String> trainingNames = new List<String>();

        /// <summary>
        /// trainingFaceInfo
        /// </summary>
        private List<FaceInfo> trainingFaceInfo = new List<FaceInfo>();

        /// <summary>
        /// trainingFaceInfoDict
        /// </summary>
        private Dictionary<String, FaceInfo> trainingFaceInfoDict = new Dictionary<string, FaceInfo>();

        #endregion private members

        /// <summary>
        ///  wczytanie zdjec i wczytanie info o osobach
        /// </summary>
        public FaceRecognizer()
        {
            // create regex
            Regex regex = new Regex(@"(\w+) \(\d+\).jpg");

            // read face images from folder
            foreach (string filename in Directory.EnumerateFiles("faces", "*.jpg"))
            {
                // parse label
                Match match = regex.Match(filename);

                // read person name
                string personName = match.Groups[1].Value;

                // add image to list
                trainingImages.Add(new Image<Gray, byte>(filename));
                trainingNames.Add(personName);

                // increment number of train images
                numTrainImages++;
            }

            // create records
            foreach (string name in trainingNames)
            {
                // add record
                trainingFaceInfoDict[name] = new FaceInfo(name);
            }

            // read info about ppl
            String text = File.ReadAllText("faces/info.txt");

            // split text to lines
            String[] lines = text.Split('\n');

            // iterate lines
            foreach (string line in lines)
            {
                // skip empty line
                if (line.Length == 0) continue;

                // split line
                String[] values = line.Split(';');

                // get id
                String name = values[0];

                // create key if not exists
                if (!trainingFaceInfoDict.ContainsKey(name))
                {
                    trainingFaceInfoDict[name] = new FaceInfo(name);
                }

                // get record
                FaceInfo faceInfo = trainingFaceInfoDict[name];

                // set values
                faceInfo.FirstName = values[1];
                faceInfo.LastName = values[2];
                faceInfo.Age = values[3];
                faceInfo.Sex = values[4];
                faceInfo.Glasses = values[5];
                faceInfo.SkinColor = values[6];
                faceInfo.Beard = values[7];
                faceInfo.HairSize = values[8];
            }
        }

        /// <summary>
        /// recognizeFaces
        /// </summary>
        public int recognizeFaces(out FaceInfo faceInfo)
        {
            // init
            faceInfo = new FaceInfo();

            // restore original image
            CurrentImage = _originalImage;

            // number of detected faces
            int numFacesDetected = 0;

            // create face Detector
            MCvAvgComp[][] facesDetected = CurrentImage.DetectHaarCascade(faceHaar, 1.2, 10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(10, 10));

            // iterate detected faces list
            foreach (MCvAvgComp f in facesDetected[0])
            {
                // resize and convert image
                Image<Gray, Byte> result = CurrentImage.Copy(f.rect)
                    .Convert<Gray, byte>()
                    .Resize(92, 112, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                // draw rectangle around face
                CurrentImage.Draw(f.rect, new Bgr(Color.Red), 2);

                // recognize faces
                if (trainingImages.ToArray().Length != 0)
                {
                    // TermCriteria for face recognition with numbers of trained images like maxIteration
                    MCvTermCriteria termCrit = new MCvTermCriteria(numTrainImages, 0.001);

                    // Eigen face recognizer
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(), trainingNames.ToArray(), 3000, ref termCrit);

                    // get name of the face
                    String name = recognizer.Recognize(result);

                    // get faceInfo
                    if (name.Length != 0)
                    {
                        if (!trainingFaceInfoDict.ContainsKey(name))
                        {
                            trainingFaceInfoDict[name] = new FaceInfo(name);
                        }

                        faceInfo = trainingFaceInfoDict[name];
                    }

                    // draw the label for each face detected and recognized
                    CurrentImage.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightBlue));
                }

                // increase detected faces number
                numFacesDetected++;
            }

            // return number of faces detected
            return numFacesDetected;
        }

        /// <summary>
        /// addFace
        /// </summary>
        public void addFace(string name, Image<Gray, Byte> image)
        {
            // add image
            this.trainingImages.Add(image);

            // add name
            this.trainingNames.Add(name);

            // increase number of train images
            numTrainImages++;

            // save bitmap
            image.Bitmap.Save(generateFileName(name), ImageFormat.Jpeg);
        }

        /// <summary>
        /// getTrainFaces
        /// </summary>
        public List<Image<Gray, Byte>> getTrainFaces()
        {
            // create face list
            List<Image<Gray, Byte>> faces = new List<Image<Gray, Byte>>();

            // create face Detector
            MCvAvgComp[][] facesDetected = CurrentImage.DetectHaarCascade(faceHaar, 1.2, 10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(10, 10));

            // iterate detected faces list
            foreach (MCvAvgComp f in facesDetected[0])
            {
                // convert and resize face image
                Image<Gray, Byte> face = CurrentImage.Copy(f.rect)
                    .Convert<Gray, byte>()
                    .Resize(92, 112, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                // add face to list
                faces.Add(face);
            }

            // return list
            return faces;
        }

        /// <summary>
        /// generateFileName
        /// </summary>
        private String generateFileName(string name)
        {
            int i = 0;

            foreach (String s in trainingNames)
            {
                if (s.Equals(name))
                {
                    i++;
                }
            }

            return "faces/" + name + " (" + i + ").jpg";
        }
    }
}
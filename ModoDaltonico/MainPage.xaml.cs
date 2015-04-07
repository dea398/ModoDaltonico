using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//los nuevos namespace que debemos agregar
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;

namespace ModoDaltonico
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Quién nos informará la cantidad de bytes por pixel
        /// </summary>
        private readonly uint bytesPerPixel;
        /// <summary>
        /// Kinect Activo
        /// </summary>
        private KinectSensor kinectSensor = null;
        /// <summary>
        /// Quién va a leer por los frames de color
        /// </summary>
        private ColorFrameReader colorFrameReader = null;
        /// <summary>
        /// Bitmap que se dibujará en pantalla
        /// </summary>
        private WriteableBitmap bitmap = null;

        private WriteableBitmap colorBmp = null;
        /// <summary>
        /// Donde almacenaremos en forma de bytes la info del Color del Kinect
        /// </summary>
        private byte[] colorPixels = null;
        byte[] picturePixels = null;

        public MainPage()
        {
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();
            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;
            // create the colorFrameDescription from the ColorFrameSource using rgba format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            // rgba is 4 bytes per pixel
            this.bytesPerPixel = colorFrameDescription.BytesPerPixel;
            // allocate space to put the pixels to be rendered
            this.colorPixels = new byte[colorFrameDescription.Width * colorFrameDescription.Height * this.bytesPerPixel];

            this.picturePixels = new byte[colorFrameDescription.Width * colorFrameDescription.Height * this.bytesPerPixel];

            // create the bitmap to display
            this.bitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height);
            this.colorBmp = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height);

            // open the sensor
            this.kinectSensor.Open();
            // initialize the components (controls) of the window
            this.InitializeComponent();
            theImage.Source = this.bitmap;
            thePicture.Source = this.colorBmp;
        }


        void colorFrameReader_FrameArrived(ColorFrameReader sender, ColorFrameArrivedEventArgs args)
        {
            bool colorFrameProcessed = false;
            // ColorFrame es IDisposable por eso usaremos el using, de igual forma, adquirimos el frame que nos esta llegando en esta fracción de segundo
            using (ColorFrame colorFrame = args.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    // verificamos que la información del colorFrame, si tenga el tamaño que esperamos.
                    if ((colorFrameDescription.Width == this.bitmap.PixelWidth) && (colorFrameDescription.Height == this.bitmap.PixelHeight))
                    {
                        if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)//verificamos que la información este en el formato que esperamos
                        {
                            colorFrame.CopyRawFrameDataToArray(this.colorPixels);
                        }
                        else
                        {
                            colorFrame.CopyConvertedFrameDataToArray(this.colorPixels, ColorImageFormat.Bgra);//de lo contrario la convertimos a BGRA
                        }

                        //Nuevo código empieza aquí
                        ChangeColorPixels();
                        //Termina aquí

                        colorPixels.CopyTo(this.bitmap.PixelBuffer);//copiamos los bytes que representan a la imagen en el buffer del bitmap
                        colorFrameProcessed = true;
                    }
                }
            }
            // como todo anduvo muy bien, le decimos al bitmap que se redibuje
            if (colorFrameProcessed)
            {
                this.bitmap.Invalidate();
            }
        }


        private void ChangeColorPixels()
        {            
            //Noten, que voy aumentando i, 4 veces por ciclo terminado, esto dado a que 4 bytes representan al pixel, es decir, recorreré son los
            //pixeles
            
            for (int i = 0; i < colorPixels.Length; i+=(int)this.bytesPerPixel)
            {
                // posición i será azul, posición i+1 será verde, posición i+2 será rojo
                //Ahora preguntamos si los canales BGR conforman Rojo                              

                if (colorPixels[i] < 75 && colorPixels[i + 1] < 75 && colorPixels[i + 2] > 100)
                {
                    colorPixels[i] = 0;
                    colorPixels[i + 1] = 255;
                    colorPixels[i + 2] = 255;
                }
               
                
            }
        }

        private void RedDetection()
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            colorPixels.CopyTo(picturePixels,0);
            //RedDetection();
            this.colorBmp.Invalidate();
           
        }
    }
}

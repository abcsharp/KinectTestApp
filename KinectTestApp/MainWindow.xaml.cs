using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using Microsoft.Kinect;
using System.Globalization;

namespace KinectTestApp
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private KinectSensor sensor;
		private WriteableBitmap bitmap;
		private byte[] bitmappixels;
		private Skeleton[] skeletons;
		private DrawingGroup drawingGroup;
		private DrawingImage drawingImage;
		private Int32Rect updateRect;
		private Rect drawingRect;
		private GeometryButton button1,button2;
		private JointIntersection intersect1,intersect2;
		private JointDistance distance1;
		private HandVolume volume1;
		
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			drawingGroup=new DrawingGroup();
			drawingImage=new DrawingImage(drawingGroup);
			screen.Source=drawingImage;
			var connectedSensors=(from s in KinectSensor.KinectSensors
								  where s.Status==KinectStatus.Connected select s).ToArray();
			if(connectedSensors.Length==0){
				MessageBox.Show("Kinect is not ready!","KinectTestApp",MessageBoxButton.OK,MessageBoxImage.Error);
				Close();
				return;
			}
			sensor=connectedSensors[0];
			sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
			sensor.SkeletonStream.Enable();
			bitmappixels=new byte[sensor.ColorStream.FramePixelDataLength];
			skeletons=new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
			bitmap=new WriteableBitmap(sensor.ColorStream.FrameWidth,sensor.ColorStream.FrameHeight,96.0,96.0,PixelFormats.Bgr32,null);
			updateRect=new Int32Rect(0,0,sensor.ColorStream.FrameWidth,sensor.ColorStream.FrameHeight);
			drawingRect=new Rect(0.0,0.0,sensor.ColorStream.FrameWidth,sensor.ColorStream.FrameHeight);
			sensor.AllFramesReady+=AllFramesReady;
			drawingGroup.ClipGeometry=new RectangleGeometry(drawingRect);
			button1=new GeometryButton(sensor,new RectangleGeometry(new Rect(0.0,0.0,80.0,480.0)),new []{JointType.HandLeft,JointType.HandRight});
			button2=new GeometryButton(sensor,new RectangleGeometry(new Rect(560.0,0.0,80.0,480.0)),new []{JointType.HandRight,JointType.HandLeft});
			intersect1=new JointIntersection(sensor,JointType.HandLeft,new []{JointType.KneeLeft,JointType.KneeRight});
			intersect2=new JointIntersection(sensor,JointType.HandRight,new []{JointType.KneeLeft,JointType.KneeRight});
			distance1=new JointDistance(sensor,JointType.HandLeft,JointType.HandRight);
			volume1=new HandVolume(sensor);
			//button1.JointHitting+=Ring;
			//button2.JointHitting+=Ring;
			//intersect1.JointIntersect+=Ring2;
			//intersect2.JointIntersect+=Ring2;
			try{
				sensor.Start();
			}catch(IOException){
				MessageBox.Show("Error detected!","KinectTestApp",MessageBoxButton.OK,MessageBoxImage.Error);
				Close();
			}
			return;
		}

		private void AllFramesReady(object sender,AllFramesReadyEventArgs e)
		{
			using(ColorImageFrame colorImage=e.OpenColorImageFrame())
				using(SkeletonFrame skeletonFrame=e.OpenSkeletonFrame())
					if(colorImage!=null&&skeletonFrame!=null){
						colorImage.CopyPixelDataTo(bitmappixels);
						skeletonFrame.CopySkeletonDataTo(skeletons);
						bitmap.WritePixels(updateRect,bitmappixels,bitmap.PixelWidth*sizeof(int),0);
						using(DrawingContext drawingContext=drawingGroup.Open()){
							drawingContext.DrawImage(bitmap,drawingRect);
							//drawingContext.DrawGeometry(button1.IsHitting?Brushes.White:null,new Pen(Brushes.Blue,2.0),button1.Geometry);
							//drawingContext.DrawGeometry(button2.IsHitting?Brushes.White:null,new Pen(Brushes.Blue,2.0),button2.Geometry);
							foreach(Skeleton skel in skeletons){
								if(skel.TrackingState==SkeletonTrackingState.Tracked){
									foreach(Joint joint in skel.Joints){
										if(joint.TrackingState==JointTrackingState.Tracked){
											var depthPoint=sensor.MapSkeletonPointToDepth(joint.Position,DepthImageFormat.Resolution640x480Fps30);
											drawingContext.DrawEllipse(Brushes.Green,null,new Point(depthPoint.X,depthPoint.Y),15,15);
										}
									}
									drawingContext.DrawRectangle(Brushes.Red,null,new Rect(0.0,0.0,distance1.Distance,50.0));
									drawingContext.DrawLine(new Pen(Brushes.Blue,10),volume1.MiddlePoint,volume1.RightHandLocation);
									var mat=Matrix.Identity;
									mat.RotateAt(volume1.Angle,volume1.MiddlePoint.X,volume1.MiddlePoint.Y);
									drawingContext.DrawLine(new Pen(Brushes.Blue,10),volume1.MiddlePoint,mat.Transform(volume1.RightHandLocation));
									drawingContext.DrawText(new FormattedText(volume1.Angle.ToString(),CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("MS Gothic"),150,Brushes.Blue),new Point());
									break;
								}
							}
						}
					}
			return;
		}

		private void Window_Closing(object sender,System.ComponentModel.CancelEventArgs e)
		{
			if(sensor!=null) sensor.Stop();
			return;
		}

		private void Ring(object sender,JointHittingEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback((_)=>
				{
					var player=new System.Media.SoundPlayer("kick.wav");
					player.PlaySync();
					player.Dispose();
				}));
			return;
		}

		private void Ring2(object sender,JointIntersectEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback((_)=>
				{
					var player=new System.Media.SoundPlayer("kick.wav");
					player.PlaySync();
					player.Dispose();
				}));
			return;
		}
	}
}

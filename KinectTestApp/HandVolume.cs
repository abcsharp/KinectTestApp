using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Kinect;

namespace KinectTestApp
{
	class HandVolume
	{
		protected KinectSensor sensor;

		protected void MeasureAngle(object sender,SkeletonFrameReadyEventArgs e)
		{
			using(SkeletonFrame skeletonFrame=e.OpenSkeletonFrame())
				if(skeletonFrame!=null){
					Skeleton[] skeletons=new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(skeletons);
					Skeleton firstTrackedSkeleton=null;
					foreach(Skeleton skel in skeletons)
						if(skel.TrackingState==SkeletonTrackingState.Tracked){
 							firstTrackedSkeleton=skel;
							break;
						}
					if(firstTrackedSkeleton==null){
						IsTracking=false;
						return;
					}
					IsTracking=true;
					var point1=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[JointType.HandRight].Position,DepthImageFormat.Resolution640x480Fps30);
					var point2=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[JointType.HandLeft].Position,DepthImageFormat.Resolution640x480Fps30);
					RightHandLocation=new Point(point1.X,point1.Y);
					LeftHandLocation=new Point(point2.X,point2.Y);
					MiddlePoint=new Point((point1.X+point2.X)/2.0,(point1.Y+point2.Y)/2.0);
					var angle=(int)(Math.Atan(-(((double)point2.Y-point1.Y)/(point2.X-point1.X)))*(180.0/Math.PI));
					Angle=point1.X<point2.X?point1.Y<point2.Y?angle+180:angle-180:angle;
				}
			return;
		}

		public bool IsTracking{get;protected set;}
		public int Angle{get;protected set;}
		public Point MiddlePoint{get;protected set;}
		public Point RightHandLocation{get;protected set;}
		public Point LeftHandLocation{get;protected set;}

		public HandVolume(KinectSensor sensor)
		{
			if(sensor==null) throw new ArgumentNullException();
			this.sensor=sensor;
			sensor.SkeletonFrameReady+=MeasureAngle;
			IsTracking=false;
			Angle=0;
			MiddlePoint=new Point();
			RightHandLocation=new Point();
			LeftHandLocation=new Point();
			return;
		}

	}
}

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
	class JointDistance
	{
		protected KinectSensor sensor;
		
		protected void MeasureDistance(object sender,SkeletonFrameReadyEventArgs e)
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
					var point1=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[Joint1].Position,DepthImageFormat.Resolution640x480Fps30);
					var point2=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[Joint2].Position,DepthImageFormat.Resolution640x480Fps30);
					Joint1Location=new Point(point1.X,point1.Y);
					Joint2Location=new Point(point2.X,point2.Y);
					Distance=(int)Math.Sqrt((point2.X-point1.X)*(point2.X-point1.X)+(point2.Y-point1.Y)*(point2.Y-point1.Y));
				}
			return;
		}

		public bool IsTracking{get;set;}
		public JointType Joint1{get;set;}
		public JointType Joint2{get;set;}
		public Point Joint1Location{get;protected set;}
		public Point Joint2Location{get;protected set;}
		public int Distance{get;protected set;}

		public JointDistance(KinectSensor sensor,JointType joint1,JointType joint2)
		{
			if(sensor==null) throw new ArgumentNullException();
			this.sensor=sensor;
			sensor.SkeletonFrameReady+=MeasureDistance;
			IsTracking=false;
			Joint1=joint1;
			Joint2=joint2;
			Joint1Location=new Point();
			Joint2Location=new Point();
			Distance=0;
			return;
		}
	}
}

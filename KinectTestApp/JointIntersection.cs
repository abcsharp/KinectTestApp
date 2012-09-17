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
	class JointIntersectEventArgs:EventArgs
	{
		public JointType Joint{get;set;}

		public JointIntersectEventArgs(JointType joint)
		{
			Joint=joint;
			return;
		}
	}

	class JointIntersection
	{
		protected KinectSensor sensor;

		protected void IntersectTest(object sender,SkeletonFrameReadyEventArgs e)
		{
			if(IsEnabled)
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
							Intersects=false;
							return;
						}
						int intersectJointCount=0;
						var firstIntersectJoint=JointType.Head;
						var depthImagePoint=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[MainJoint].Position,DepthImageFormat.Resolution640x480Fps30);
						var mainJointRect=JointBounds;
						mainJointRect.Offset(depthImagePoint.X,depthImagePoint.Y);
						foreach(JointType joint in DestJoints){
							depthImagePoint=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[joint].Position,DepthImageFormat.Resolution640x480Fps30);
							var destJointRect=JointBounds;
							destJointRect.Offset(depthImagePoint.X,depthImagePoint.Y);
							var IntersectTestResult=mainJointRect.IntersectsWith(destJointRect);
							if(IntersectTestResult){
								if(intersectJointCount==0) firstIntersectJoint=joint;
								intersectJointCount++;
							}
						}
						if(intersectJointCount>0&&!Intersects){
							Intersects=true;
							OnIntersect(new JointIntersectEventArgs(firstIntersectJoint));
						}else if(intersectJointCount==0&&Intersects){
							Intersects=false;
							OnLeave();
						}
					}
			return;
		}

		protected void OnIntersect(JointIntersectEventArgs e)
		{
			if(e!=null&&JointIntersect!=null) JointIntersect(this,e);
		}

		protected void OnLeave()
		{
			if(JointLeave!=null) JointLeave(this,new EventArgs());
		}

		public bool IsEnabled{get;set;}
		public JointType MainJoint{get;set;}
		public IEnumerable<JointType> DestJoints{get;set;}
		public bool Intersects{get;protected set;}
		public Rect JointBounds{get;set;}
		public event EventHandler<JointIntersectEventArgs> JointIntersect;
		public event EventHandler JointLeave;

		public JointIntersection(KinectSensor sensor,JointType mainJoint,IEnumerable<JointType> destJoints)
		{
			if(sensor==null) throw new ArgumentNullException();
			if(destJoints==null) throw new ArgumentNullException();
			this.sensor=sensor;
			sensor.SkeletonFrameReady+=IntersectTest;
			MainJoint=mainJoint;
			DestJoints=destJoints;
			IsEnabled=true;
			Intersects=false;
			JointBounds=new Rect(-30.0,-30.0,60.0,60.0);
			return;
		}

		public JointIntersection(KinectSensor sensor,JointType mainJoint,IEnumerable<JointType> destJoints,Rect jointBounds)
		{
			if(sensor==null) throw new ArgumentNullException();
			if(destJoints==null) throw new ArgumentNullException();
			if(jointBounds==null) throw new ArgumentNullException();
			this.sensor=sensor;
			sensor.SkeletonFrameReady+=IntersectTest;
			MainJoint=mainJoint;
			DestJoints=destJoints;
			IsEnabled=true;
			Intersects=false;
			JointBounds=jointBounds;
			return;
		}
	}
}

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
	class JointHittingEventArgs:EventArgs
	{
		public JointType Joint{get;set;}

		public JointHittingEventArgs(JointType joint)
		{
			Joint=joint;
			return;
		}
	}

	class GeometryButton
	{
		protected KinectSensor sensor;

		protected void HitTest(object sender,SkeletonFrameReadyEventArgs e)
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
							IsHitting=false;
							return;
						}
						int hittingJointCount=0;
						JointType firstHittingJoint=JointType.Head;
						foreach(JointType joint in Joints){
							var depthImagePoint=sensor.MapSkeletonPointToDepth(firstTrackedSkeleton.Joints[joint].Position,DepthImageFormat.Resolution640x480Fps30);
							var HitTestResult=Geometry.FillContains(new Point(depthImagePoint.X,depthImagePoint.Y));
							if(HitTestResult){
								if(hittingJointCount==0) firstHittingJoint=joint;
								hittingJointCount++;
							}
						}
						if(hittingJointCount>0&&!IsHitting){
							IsHitting=true;
							OnHitting(new JointHittingEventArgs(firstHittingJoint));
						}else if(hittingJointCount==0&&IsHitting){
							IsHitting=false;
							OnLeave();
						}
					}
			return;
		}

		protected void OnHitting(JointHittingEventArgs e)
		{
			if(e!=null&&JointHitting!=null) JointHitting(this,e);
		}

		protected void OnLeave()
		{
			if(JointLeave!=null) JointLeave(this,new EventArgs());
		}

		public bool IsEnabled{get;set;}
		public IEnumerable<JointType> Joints{get;set;}
		public bool IsHitting{get;protected set;}
		public Geometry Geometry{get;protected set;}
		public event EventHandler<JointHittingEventArgs> JointHitting;
		public event EventHandler JointLeave;

		public GeometryButton(KinectSensor sensor,Geometry buttonGeometry,IEnumerable<JointType> joints)
		{
			IsEnabled=true;
			if(sensor==null) throw new ArgumentNullException();
			if(buttonGeometry==null) throw new ArgumentNullException();
			this.sensor=sensor;
			sensor.SkeletonFrameReady+=HitTest;
			if(joints==null) Joints=new List<JointType>();
			else Joints=joints;
			Geometry=buttonGeometry;
			IsHitting=false;
			return;
		}
	}
}

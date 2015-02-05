using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Scanning
{
    // Tuples are immutable
    using Pixel = Tuple<byte, byte, byte>;

    // A scene view is a view of a scene from a particular origin with a viewing direction and 
    // field of view. It also includes the array of pixels that make up the projection of the scene 
    // from this perspective. A scene perspective is immutable save for mutability intentionally 
    // exposed by the Image class for efficiency; documented there.
    public class SceneView
    {
        // The image data for the perspective. Byte 0 is at the top-left. Values must not
        // be modified
        public Image Image { get { return img; } }

        //  The focal point of this scene perspective, as a 3D vector of doubles
        public Point3Df CenterOfProjection { get { return centerOfProjection; } }

        // The direction this view faces, as a 3D vector of doubles
        public Point3Df Orientation { get { return orientation; } }

        // The vertical field of view of this perspective, in radians
        public double VerticalFOV { get { return fieldOfView.Y; } }
        // The horizontal field of view of this perspective, in radians
        public double HorizontalFOV { get { return fieldOfView.X; } }

        // The distance from the center of projection to the image plane, in pixels. Precalculated
        // from the image data dimensions and FOV.
        public double DistanceToImagePlane { get { return distanceToImagePlane; } }

        // The private variables behind the above properties.
        private readonly Image img;
        private readonly Point3Df centerOfProjection;
        private readonly Point3Df orientation;
        private readonly Point2Df fieldOfView;
        private readonly double distanceToImagePlane;

        public SceneView(Image img, Point3Df centerOfProjection, 
            Point3Df orientation, double verticalFOV, double horizontalFOV)
        {
            this.img = img;

            this.centerOfProjection = centerOfProjection;
            this.orientation = orientation;

            this.fieldOfView = new Point2Df(horizontalFOV, verticalFOV);

            // CHECKME check vertical FOV vs horizontal FOV. Horizontal pixels are often a different
            // CHECKME height - 1?
            // size than vertical pixels, but we're trying to use them as our unit in 3D space as
            // well so we have to make sure we're consistent.
            // Precompute this distance based on the image size and FOV
            this.distanceToImagePlane = ((double)img.Height / 2) / Math.Tan(verticalFOV / 2);
        }            

        // Projects a pixel in this view to world coordinates at the passed depth. Depth is 
        // measured by z-coordinate. The passed pixel may be fractional, and it does not have to be 
        // within the FOV of this view.
        public Point3Df ProjectToWorld(Point2Df ptToProject, double depth)
        {
            // The 3D position of the pixel in question, in the coordinate system of the camera
            Point3Df projectedPt = ConvertToWorld3D(ptToProject);

            // Projection scale factor (for projecting out into the 3D space along a line of
            // possible depths).
            double projFactor = depth / projectedPt.Z;

            // The 3D position of a point far along the line projected through the pixel being
            // considered. It is at a maximum depth of maxDepth.
            return projectedPt * projFactor;
        }

        // Project a 3D coordinate onto this view's image plane. The returned point is in the 
        // coordinate system of the image plane, with the origin at the top-left. The result will 
        // be fractional if the world coordinate doesn't project precisely onto the center of a 
        // pixel. If the z-coordinate of the passed point is zero, the returned point will have +/- 
        // infinity coordinate values.
        public Point2Df ProjectFromWorld(Point3Df ptToProject)
        {
            Point2Df projectedPt = 
                ((Point2Df)ptToProject * (DistanceToImagePlane / ptToProject.Z));

            return ConvertFromWorld(projectedPt);
        }

        public Point3Df TransformToView(Point3Df ptToTransform, SceneView target)
        {
            // Translate
            Point3Df transformed = 
                ptToTransform + this.CenterOfProjection - target.CenterOfProjection;

            // Rotate
            if (this.Orientation != target.Orientation)
            {
                Point3Df axis = target.Orientation.Cross(this.Orientation);
                double angle = Math.Acos(this.Orientation.Dot(target.Orientation) / 
                               (this.Orientation.Length() * target.Orientation.Length()));

                transformed = transformed.Rotate(axis, angle);
            }

            return transformed;
        }

        // Given a pixel in this view and an assumed depth of the 3D point represented by that
        // pixel, returns the point in the given view where the same 3D point will be represented
        public Point2Df FindInOtherView(Point2Df pixel, SceneView target, double depth)
        {
            // Project pixel into 3D space
            Point3Df worldPt = this.ProjectToWorld(pixel, depth);

            // Transform the point into the target view's coordinate system
            Point3Df targetPt = this.TransformToView(worldPt, target);

            // Project the transformed point into the target view and return the result
            return target.ProjectFromWorld(targetPt);
        }

        // CHECKME add version of this method that returns a Point2Df so we don't have to cast?
        // Converts the passed image location in a top-left coordinate system to the equivalent
        // world coordinates
        public Point3Df ConvertToWorld3D(Point2Df imageLoc)
        {
            return new Point3Df(ConvertToWorld2D(imageLoc),
                                DistanceToImagePlane);
        }

        public Point2Df ConvertToWorld2D(Point2Df imageLoc)
        {
            return new Point2Df(imageLoc.X - (double)(Image.Width - 1) / 2,
                                (double)(Image.Height - 1) / 2 - imageLoc.Y);
        }

        // Assumes the passed point is on the image plane and converts it to a 2D point in the
        // top-left coordinate system of this view's image
        public Point2Df ConvertFromWorld(Point2Df worldLoc)
        {
            return new Point2Df(worldLoc.X + (double)(Image.Width - 1) / 2,
                                (double)(Image.Height - 1) / 2 - worldLoc.Y);
        }
    }
}

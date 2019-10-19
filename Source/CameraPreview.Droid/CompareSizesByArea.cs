using Android.Util;
using Java.Util;

namespace CameraPreview.Droid
{
    public class CompareSizesByArea : Java.Lang.Object, IComparator
    {
        public int Compare(Java.Lang.Object lhs, Java.Lang.Object rhs)
        {
            var lhsSize = (Size) lhs;
            var rhsSize = (Size) rhs;
            // We cast here to ensure the multiplications won't overflow
            return Java.Lang.Long.Signum((long) lhsSize.Width * lhsSize.Height - (long) rhsSize.Width * rhsSize.Height);
        }
    }
}
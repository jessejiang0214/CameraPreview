using System;
using System.Collections.Generic;

namespace CameraPreview
{
    public class ScanningOptionsBase
    {
        /// <summary>
        /// Camera resolution selector delegate, must return the selected Resolution from the list of available resolutions
        /// </summary>
        public delegate CameraResolution CameraResolutionSelectorDelegate(List<CameraResolution> availableResolutions);

        public ScanningOptionsBase()
        {
            this.DelayBetweenAnalyzingFrames = 150;
            this.InitialDelayBeforeAnalyzingFrames = 300;
            this.DelayBetweenContinuousScans = 1000;
        }

        public CameraResolutionSelectorDelegate CameraResolutionSelector { get; set; }

        public bool? UseFrontCameraIfAvailable { get; set; }

        public int DelayBetweenContinuousScans { get; set; }

        public int DelayBetweenAnalyzingFrames { get; set; }
        public int InitialDelayBeforeAnalyzingFrames { get; set; }

        public static ScanningOptionsBase Default
        {
            get { return new ScanningOptionsBase(); }
        }

        public CameraResolution GetResolution(List<CameraResolution> availableResolutions)
        {
            CameraResolution r = null;

            var dg = CameraResolutionSelector;

            if (dg != null)
            {
                r = dg(availableResolutions);
            }

            return r;
        }
    }
}

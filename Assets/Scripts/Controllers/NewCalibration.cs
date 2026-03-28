using UnityEngine;

namespace Fr.ImtAtlantique.CEXIHA.Core
{
    public class Calibration
    {
        /// <summary>
        /// Move (rotation and position) <paramref name="objectToCalibrate"/> so that the <paramref name="poseToAlign"/> pose matches its <paramref name="referencePose"/> pose in the world. Important : <paramref name="poseToAlign"/> must be a descendant of <paramref name="objectToCalibrate"/> for this method to work.
        /// </summary>
        /// <param name="objectToCalibrate"> The GameObject that will be move to make <paramref name="poseToAlign"/> pose matching with <paramref name="referencePose"/> pose. </param>
        /// <param name="poseToAlign"> The transform, descendant of <paramref name="objectToCalibrate"/>, corresponding to the pose that must match the <paramref name="referencePose"/> in the scene. </param>
        /// <param name="referencePose"> The transform used as a ground truth pose in the world. </param>
        public static void Calibrate(Transform objectToCalibrate, Transform poseToAlign, Transform referencePose)
        {
            // Step 1 : Calculation of true local position taking into account scale
            Vector3 scaledExpectedLocalPos = Vector3.Scale(poseToAlign.localPosition, objectToCalibrate.localScale);

            // Step 2 : Rotation
            objectToCalibrate.rotation = referencePose.rotation * Quaternion.Inverse(poseToAlign.localRotation);

            // Step 3 : Apply position
            Vector3 position = referencePose.position - objectToCalibrate.rotation * scaledExpectedLocalPos;

            objectToCalibrate.position = position;
        }
    }
}

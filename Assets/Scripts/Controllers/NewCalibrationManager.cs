using UnityEngine;

namespace Fr.ImtAtlantique.CEXIHA.Core
{
    public class CalibrationManager : MonoBehaviour
    {
        [Header("Origin of the elements to calibrate")]
        [SerializeField][Tooltip("The GameObject that will be displaced to make poseToAlign pose matching with referencePose pose.")] 
        public Transform objectToCalibrate;

        [Header("Matching poses")]
        [SerializeField][Tooltip("The transform, descendant of objectToCalibrate, corresponding to the pose that must match the referencePose in the scene.")] 
        public Transform poseToAlign;
        [SerializeField][Tooltip("The transform used as a ground truth pose in the world.")] 
        public Transform referencePose;

        [Header("Settings")]
        [SerializeField] private bool continuousCalibration;
        [SerializeField] private bool showLogWarning = false;

        private bool firstCalibrationDone = false;


        // Update is called once per frame
        void Update()
        {
            if (continuousCalibration ||!firstCalibrationDone)
            {
                try
                {
                    Calibrate();
                    firstCalibrationDone = true;
                }
                catch (System.Exception)
                {
                    return;
                }
            }
        }


        public void Calibrate()
        {
            try
            {
                CheckParameters();
            } 
            catch(System.Exception e)
            {
                string errMessage = "Calibration impossible because one of the Transform value is missing : " + e;
                if(showLogWarning) Debug.LogWarning(errMessage, this);
                throw new System.Exception(errMessage);
            }

            Calibration.Calibrate(objectToCalibrate, poseToAlign, referencePose);
        }


        private void CheckParameters()
        {
            if (objectToCalibrate == null)
            {
                throw new System.Exception("Missing objectToCalibrate value.");
            }

            if (poseToAlign == null)
            {
                throw new System.Exception("Missing poseToAlign value.");
            }

            if (referencePose == null)
            {
                throw new System.Exception("Missing referencePose value.");
            }
        }
    }
}

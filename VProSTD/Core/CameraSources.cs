using Cognex.VisionPro;
using System;
using System.Collections.Generic;

namespace VProSTD
{
    /// <summary>
    /// 카메라 이미지 입력. VisionApp 은 카메라 종류와 상관없이 이것만 쓴다.
    ///   지금 : CognexCameraSource (VisionPro 가 찾는 카메라, GigE)
    ///   나중 : Euresys CameraLink 에어리어 카메라 (Euresys SDK 로 취득 → CogImage8Grey 로 변환)
    /// [안전] 카메라를 연(Open) 순간 카메라에 연결된다. 카메라 자체는 프로그램을 끝내야 해제된다 (VisionApp.ReleaseCameras).
    /// </summary>
    public interface ICameraSource : IDisposable
    {
        string CameraName { get; }   // 화면 표시용 (예: "acA2440-20gm (12345678)")
        string Serial { get; }
        string VideoFormat { get; }
        bool IsConnected { get; }

        /// <summary>노출 시간(ms). 바꿀 수 없는 카메라면 null.</summary>
        double? Exposure { get; set; }

        /// <summary>소프트웨어 트리거로 1장 취득</summary>
        ICogImage Grab();
    }

    /// <summary>
    /// 찾은 카메라 하나 (CONNECT CAMERA 탭 카메라 목록)
    /// </summary>
    public class CameraInfo
    {
        public string Name;
        public string Serial;

        public override string ToString()
        {
            return Name + " (" + Serial + ")";
        }
    }

    /// <summary>
    /// VisionPro 카메라 (CogFrameGrabbers 에 나오는 카메라).
    /// .vpp(CogAcqFifoTool) 를 불러오지 않고, CameraSettings 값으로 코드에서 직접 AcqFifo 를 만든다.
    /// </summary>
    public class CognexCameraSource : ICameraSource
    {
        private ICogFrameGrabber _frameGrabber;
        private ICogAcqFifo _fifo;

        public string VideoFormat { get; private set; }

        private CognexCameraSource(ICogFrameGrabber frameGrabber, ICogAcqFifo fifo, string videoFormat)
        {
            _frameGrabber = frameGrabber;
            _fifo = fifo;
            VideoFormat = videoFormat;
        }

        /// <summary>
        /// 연결된 카메라 목록. [안전] CogFrameGrabbers 를 만들면 카메라를 검색한다 (종료 시 해제 필요).
        /// </summary>
        public static List<CameraInfo> FindCameras()
        {
            List<CameraInfo> cameras = new List<CameraInfo>();
            CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
            foreach (ICogFrameGrabber frameGrabber in frameGrabbers)
            {
                CameraInfo info = new CameraInfo();
                info.Name = frameGrabber.Name;
                info.Serial = frameGrabber.SerialNumber;
                cameras.Add(info);
            }
            return cameras;
        }

        /// <summary>
        /// 카메라 열기: 시리얼로 찾고(비어 있으면 첫 번째), 영상 형식을 정해 AcqFifo 를 만든다.
        /// </summary>
        public static CognexCameraSource Open(CameraSettings settings)
        {
            CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
            if (frameGrabbers.Count == 0) throw new InvalidOperationException("연결된 카메라가 없습니다.");

            ICogFrameGrabber frameGrabber = FindBySerial(frameGrabbers, settings.Serial);
            string videoFormat = ChooseVideoFormat(frameGrabber, settings.VideoFormat);

            ICogAcqFifo fifo = frameGrabber.CreateAcqFifo(videoFormat, CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);
            try
            {
                fifo.Timeout = settings.TimeoutMs > 0 ? settings.TimeoutMs : 2000;

                // 소프트웨어 트리거: Acquire() 를 부를 때 1장 찍는다
                if (fifo.OwnedTriggerParams != null)
                {
                    fifo.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Manual;
                    fifo.OwnedTriggerParams.TriggerEnabled = true;
                }
            }
            catch
            {
                VppFile.Release(fifo);
                throw;
            }

            return new CognexCameraSource(frameGrabber, fifo, videoFormat);
        }

        private static ICogFrameGrabber FindBySerial(CogFrameGrabbers frameGrabbers, string serial)
        {
            if (string.IsNullOrWhiteSpace(serial)) return frameGrabbers[0];

            foreach (ICogFrameGrabber frameGrabber in frameGrabbers)
            {
                if (string.Equals(frameGrabber.SerialNumber, serial.Trim(), StringComparison.OrdinalIgnoreCase)) return frameGrabber;
            }
            throw new InvalidOperationException("시리얼 " + serial + " 카메라를 찾을 수 없습니다. CONNECT CAMERA 탭에서 카메라를 다시 고르고 SAVE 하세요.");
        }

        private static string ChooseVideoFormat(ICogFrameGrabber frameGrabber, string wanted)
        {
            int count = frameGrabber.AvailableVideoFormats.Count;
            if (count == 0) throw new InvalidOperationException("카메라의 영상 형식을 찾을 수 없습니다.");
            if (string.IsNullOrWhiteSpace(wanted)) return frameGrabber.AvailableVideoFormats[0];

            for (int i = 0; i < count; i++)
            {
                if (frameGrabber.AvailableVideoFormats[i] == wanted) return wanted;
            }
            throw new InvalidOperationException("이 카메라에는 영상 형식 '" + wanted + "' 이(가) 없습니다.");
        }

        public string CameraName
        {
            get
            {
                if (_frameGrabber == null) return "카메라 없음";
                return _frameGrabber.Name + " (" + _frameGrabber.SerialNumber + ")";
            }
        }

        public string Serial
        {
            get { return _frameGrabber != null ? _frameGrabber.SerialNumber : ""; }
        }

        public bool IsConnected
        {
            get { return _fifo != null; }
        }

        public double? Exposure
        {
            get
            {
                if (_fifo == null || _fifo.OwnedExposureParams == null) return null;
                return _fifo.OwnedExposureParams.Exposure;
            }
            set
            {
                if (value == null) return;
                if (_fifo == null || _fifo.OwnedExposureParams == null) return;
                _fifo.OwnedExposureParams.Exposure = value.Value;
            }
        }

        public ICogImage Grab()
        {
            if (_fifo == null) throw new InvalidOperationException("카메라가 연결되어 있지 않습니다.");

            int triggerNumber;
            ICogImage image = _fifo.Acquire(out triggerNumber);
            if (image == null) throw new InvalidOperationException("이미지 취득 실패");
            return image;
        }

        public void Dispose()
        {
            VppFile.Release(_fifo);
            _fifo = null;
            _frameGrabber = null;
        }
    }
}

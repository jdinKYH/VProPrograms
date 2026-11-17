using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VProSTD
{
    /// <summary>
    /// File Mode 이미지 입력: 이미지 파일 1개 또는 폴더 안의 이미지를 차례로 읽는다.
    /// idb/cdb 처럼 파일 하나에 여러 장이 들어 있으면 안의 이미지를 차례로 읽는다.
    /// </summary>
    public class FileImageSource
    {
        private static readonly string[] Extensions = { ".bmp", ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".idb", ".cdb" };

        private List<string> _files = new List<string>();
        private int _fileIndex;      // 다음에 읽을 파일 번호
        private int _imageIndex;     // 그 파일 안에서 다음에 읽을 이미지 번호

        public string SourcePath { get; private set; }   // 고른 파일 또는 폴더

        public int FileCount
        {
            get { return _files.Count; }
        }

        /// <summary>
        /// 아직 읽지 않은 이미지가 남아 있는지
        /// </summary>
        public bool HasNext
        {
            get { return _fileIndex < _files.Count; }
        }

        public void SetSingleFile(string path)
        {
            SourcePath = path;
            _files = new List<string>();
            _files.Add(path);
            Rewind();
        }

        public void SetFolder(string folder)
        {
            SourcePath = folder;
            _files = new List<string>();
            foreach (string file in Directory.GetFiles(folder))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (Array.IndexOf(Extensions, ext) >= 0) _files.Add(file);
            }
            // img2 가 img10 보다 앞에 오도록 숫자를 고려해서 정렬
            _files.Sort((a, b) => string.Compare(SortKey(a), SortKey(b), StringComparison.OrdinalIgnoreCase));
            Rewind();
        }

        /// <summary>
        /// 처음부터 다시 읽기
        /// </summary>
        public void Rewind()
        {
            _fileIndex = 0;
            _imageIndex = 0;
        }

        /// <summary>
        /// 다음 이미지를 읽는다. 끝까지 읽었으면 처음으로 돌아간다.
        /// name: 화면/기록에 쓸 이름 (예: "img_012.bmp" 또는 "lot.idb #3")
        /// </summary>
        public ICogImage Next(out string name)
        {
            if (_files.Count == 0) throw new InvalidOperationException("이미지 파일이 없습니다. Single Image 또는 Image Folder 를 먼저 고르세요.");
            if (_fileIndex >= _files.Count) Rewind();

            string path = _files[_fileIndex];
            CogImageFile imageFile = new CogImageFile();
            try
            {
                imageFile.Open(path, CogImageFileModeConstants.Read);
                int count = imageFile.Count;

                ICogImage image = imageFile[_imageIndex];
                name = Path.GetFileName(path);
                if (count > 1) name += " #" + (_imageIndex + 1);

                // 다음 위치로 이동
                _imageIndex++;
                if (_imageIndex >= count)
                {
                    _imageIndex = 0;
                    _fileIndex++;
                }
                return image;
            }
            catch (Exception ex)
            {
                // 읽을 수 없는 파일은 건너뛰도록 다음 파일로 넘기고, 어느 파일인지 알린다
                _imageIndex = 0;
                _fileIndex++;
                throw new IOException(Path.GetFileName(path) + " 을(를) 읽을 수 없습니다: " + ex.Message, ex);
            }
            finally
            {
                // [안전] 닫지 않으면 파일이 잠긴 채로 남는다
                try
                {
                    imageFile.Close();
                }
                catch
                {
                    // 열지 못한 파일을 닫을 때 나는 오류는 무시
                }
            }
        }

        private static string SortKey(string path)
        {
            return Regex.Replace(Path.GetFileName(path), @"\d+", m => m.Value.PadLeft(10, '0'));
        }
    }

    /// <summary>
    /// 카메라 이미지 입력: Acq.vpp 의 CogAcqFifoTool 로 1장씩 취득한다.
    /// [안전] 이 객체를 만들면(불러오면) 카메라에 연결된다. 카메라는 프로그램을 끝내야 해제된다.
    /// </summary>
    public class AcqImageSource : IDisposable
    {
        public CogAcqFifoTool Tool { get; private set; }

        private AcqImageSource(CogAcqFifoTool tool)
        {
            Tool = tool;
        }

        /// <summary>
        /// Acq.vpp 불러오기
        /// </summary>
        public static AcqImageSource Load(string path)
        {
            object obj = VppFile.Load(path);
            CogAcqFifoTool tool = obj as CogAcqFifoTool;
            if (tool == null)
            {
                VppFile.Release(obj);
                throw new InvalidDataException("Acq.vpp 가 CogAcqFifoTool 파일이 아닙니다.");
            }
            return new AcqImageSource(tool);
        }

        /// <summary>
        /// Acq.vpp 가 없을 때: 연결된 첫 번째 카메라로 새로 만든다.
        /// </summary>
        public static AcqImageSource CreateFromFirstCamera()
        {
            CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
            if (frameGrabbers.Count == 0) throw new InvalidOperationException("연결된 카메라가 없습니다.");

            ICogFrameGrabber frameGrabber = frameGrabbers[0];
            if (frameGrabber.AvailableVideoFormats.Count == 0) throw new InvalidOperationException("카메라의 영상 형식을 찾을 수 없습니다.");

            string videoFormat = frameGrabber.AvailableVideoFormats[0];
            ICogAcqFifo fifo = frameGrabber.CreateAcqFifo(videoFormat, CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);

            CogAcqFifoTool tool = new CogAcqFifoTool();
            tool.Operator = fifo;
            return new AcqImageSource(tool);
        }

        public void Save(string path)
        {
            VppFile.Save(Tool, path, true);   // 취득 이미지는 저장할 필요 없으므로 Minimum
        }

        /// <summary>
        /// 카메라 이름 (예: "acA2440-20gm (12345678)")
        /// </summary>
        public string CameraName
        {
            get
            {
                if (Tool.Operator == null || Tool.Operator.FrameGrabber == null) return "카메라 없음";
                ICogFrameGrabber frameGrabber = Tool.Operator.FrameGrabber;
                return frameGrabber.Name + " (" + frameGrabber.SerialNumber + ")";
            }
        }

        /// <summary>
        /// 카메라가 실제로 연결되어 있는지 (카메라 없는 PC 에서 Acq.vpp 를 불러오면 false)
        /// </summary>
        public bool IsConnected
        {
            get { return Tool.Operator != null; }
        }

        /// <summary>
        /// 노출 시간(ms). 노출을 바꿀 수 없는 카메라면 null.
        /// </summary>
        public double? Exposure
        {
            get
            {
                if (Tool.Operator == null || Tool.Operator.OwnedExposureParams == null) return null;
                return Tool.Operator.OwnedExposureParams.Exposure;
            }
            set
            {
                if (value == null) return;
                if (Tool.Operator == null || Tool.Operator.OwnedExposureParams == null) return;
                Tool.Operator.OwnedExposureParams.Exposure = value.Value;
            }
        }

        /// <summary>
        /// 1장 취득
        /// </summary>
        public ICogImage Grab()
        {
            if (!IsConnected) throw new InvalidOperationException("카메라가 연결되어 있지 않습니다.");

            Tool.Run();
            if (Tool.RunStatus.Result != CogToolResultConstants.Accept || Tool.OutputImage == null)
            {
                throw new InvalidOperationException("이미지 취득 실패: " + Tool.RunStatus.Message);
            }
            return Tool.OutputImage;
        }

        public void Dispose()
        {
            VppFile.Release(Tool);
            Tool = null;
        }
    }
}

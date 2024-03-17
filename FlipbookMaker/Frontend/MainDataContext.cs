using FlipbookMaker.Backend;
using FlipbookMaker.Data;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.Reflection;
using System.Text.Json;
using System;
using System.Windows;

namespace FlipbookMaker.Frontend.Viewmodels
{
    internal class MainDataContext
        : Observable
    {
        public Image? PreviewImageControl { get; set; } = null;

        #region Commands
        public BaseCommand AddFrame { get; }
        public BaseCommand RemoveFrame { get; }
        public BaseCommand ClearFrames { get; }
        public BaseCommand MoveUpFrame { get; }
        public BaseCommand MoveDownFrame { get; }
        public BaseCommand PreviewMotion { get; }
        public BaseCommand BuildFlipbook { get; }
        public BaseCommand OpenCacheFile { get; }
        #endregion

        #region Bindable properties
        public static string WindowName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var name = assembly.GetName();

                return $"{name.Name} - {name.Version} by Planet";
            }
        }
        public int CurrentlySelectedFrameIndex
        {
            get => _backingCurrentlySelectedFrameIndex;
            set
            {
                NotifyPropertyChanged(ref _backingCurrentlySelectedFrameIndex, value);
                RemoveFrame.RaiseCanExecuteChanged();
                MoveUpFrame.RaiseCanExecuteChanged();
                MoveDownFrame.RaiseCanExecuteChanged();
            }
        }
        public ReadOnlyObservableCollection<FlipbookFrame> Frames
        {
            get => _backingReadonlyFrames;
            set
            {
                NotifyPropertyChanged(ref _backingReadonlyFrames, value);
            }
        }
        public int FrameSideSize => AvailableFrameSizes[SelectedFrameSizeIndex];
        public int ColumnNumber
        {
            get => _backingColumnNumber;
            set { NotifyPropertyChanged(ref _backingColumnNumber, value); }
        }
        public int SelectedFrameSizeIndex
        {
            get => _backingSelectedFrameSizeIndex;
            set { NotifyPropertyChanged(ref _backingSelectedFrameSizeIndex, value); }
        }
        public List<int> AvailableFrameSizes { get; } = new()
        {
            2,
            4,
            8,
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
        };
        #endregion

        #region Private

        private readonly ObservableCollection<FlipbookFrame> _frames = new();

        private int _backingCurrentlySelectedFrameIndex = -1;
        private ReadOnlyObservableCollection<FlipbookFrame> _backingReadonlyFrames;
        private int _backingSelectedFrameSizeIndex;
        private int _backingColumnNumber = 1;

        #endregion

        public MainDataContext()
        {
            _backingReadonlyFrames = null!;
            Frames = new ReadOnlyObservableCollection<FlipbookFrame>(_frames);

            // Commands
            AddFrame = new BaseCommand(CallbackAddFrame);
            RemoveFrame = new BaseCommand(CallbackRemoveFrame, IsCallbackRemoveFrameEnabled);
            ClearFrames = new BaseCommand(CallbackClearFrame);
            PreviewMotion = new BaseCommand(CallbackPreviewMotion);
            MoveUpFrame = new BaseCommand(CallbackMoveUpFrame, CallbackCanMoveUpFrame);
            MoveDownFrame = new BaseCommand(CallbackMoveDownFrame, CallbackCanMoveDownFrame);
            BuildFlipbook = new BaseCommand(CallbackBuildFlipbookImage, CallbackCanBuildFlipbookImage);
            OpenCacheFile = new BaseCommand(CallbackOpenCacheFile);

            PropertyChanged += MainDataContext_PropertyChanged;
        }

        #region Command callbacks
        private void CallbackOpenCacheFile()
        {
            _frames.Clear();

            OpenFileDialog diag = new()
            {
                CheckFileExists = true,
                Filter = "Flipbook cache file|*.fcache",
                Multiselect = false,
            };

            if (diag.ShowDialog() == true)
            {
                try
                {
                    string cacheFile = File.ReadAllText(diag.FileName);
                    Cache loadedCache = JsonSerializer.Deserialize<Cache>(cacheFile)!;
                    loadedCache.ValidateData();

                    if (!AvailableFrameSizes.Contains(loadedCache.FrameSize))
                    {
                        AvailableFrameSizes.Add(loadedCache.FrameSize);
                        SelectedFrameSizeIndex = AvailableFrameSizes.Count - 1;
                    }
                    else
                    {
                        SelectedFrameSizeIndex = AvailableFrameSizes.FindIndex(i => i == loadedCache.FrameSize);
                    }

                    ColumnNumber = loadedCache.ColumnNumber;
                    LoadFlipbookFrames(loadedCache.Files);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not load cache:\n{ex}");
                    return;
                }
            }
        }

        private void CallbackClearFrame() => _frames.Clear();
        private void CallbackRemoveFrame()
        {
            _frames.RemoveAt(CurrentlySelectedFrameIndex);
            RemoveFrame.RaiseCanExecuteChanged();
            BuildFlipbook.RaiseCanExecuteChanged();
        }
        private bool IsCallbackRemoveFrameEnabled(object? _)
        {
            return CurrentlySelectedFrameIndex >= 0;
        }
        private void CallbackAddFrame()
        {
            OpenFileDialog diag = new()
            {
                CheckFileExists = true,
                Filter = "PNG Image file|*.png",
                Multiselect = true,
            };

            var res = diag.ShowDialog();

            if (res == true)
            {
                LoadFlipbookFrames(diag.FileNames);
                RemoveFrame.RaiseCanExecuteChanged();
                BuildFlipbook.RaiseCanExecuteChanged();
            }
        }
        private void LoadFlipbookFrames(ICollection<string> files)
        {
            foreach (var f in files)
            {
                _frames.Add(new FlipbookFrame(f, File.ReadAllBytes(f)));
            }

            BuildFlipbook.RaiseCanExecuteChanged();
        }
        private void CallbackMoveUpFrame()
        {
            var index = CurrentlySelectedFrameIndex;
            FlipbookFrame frame = _frames[index];
            _frames.RemoveAt(index);
            _frames.Insert(index - 1, frame);
            CurrentlySelectedFrameIndex = index - 1;
        }
        private void CallbackMoveDownFrame()
        {
            var index = CurrentlySelectedFrameIndex;
            FlipbookFrame frame = _frames[index];
            _frames.RemoveAt(index);

            _frames.Insert(index + 1, frame);
            CurrentlySelectedFrameIndex = index + 1;
        }
        private bool CallbackCanMoveUpFrame(object? _)
        {
            return CurrentlySelectedFrameIndex > 0;
        }
        private bool CallbackCanMoveDownFrame(object? _)
        {
            return CurrentlySelectedFrameIndex < _frames.Count - 1;
        }
        private void CallbackPreviewMotion(object? imageControl)
        {
            if (_frames.Count == 0)
                return;

            RegeneratePreviewStream(-1);
        }
        private void CallbackBuildFlipbookImage()
        {
            ImageProcessor process = new();

            SaveFileDialog diag = new()
            {
                Filter = "PNG Image file|*.png",
            };

            if (diag.ShowDialog() == true)
            {
                process.CreateFlipbookImage(_frames, ColumnNumber, FrameSideSize, diag.FileName);

                Cache cache = new()
                {
                    FrameSize = FrameSideSize,
                    Files = _frames.Select(f => f.Filepath).ToList(),
                    ColumnNumber = ColumnNumber,
                };

                string ss = System.Text.Json.JsonSerializer.Serialize(cache);
                var path = Path.ChangeExtension(diag.FileName, "fcache");
                File.WriteAllText(path, ss);
            }
        }
        private bool CallbackCanBuildFlipbookImage(object? _)
        {
            return _frames.Count > 0;
        }
        #endregion

        private void RegeneratePreviewStream(int frameIndex)
        {
            List<byte[]> frames = new();

            if (frameIndex < 0)
                frames.AddRange(_frames.Select(f => f.Image));
            else
                frames.Add(_frames[frameIndex].Image);


            GifBitmapEncoder gEnc = new();

            foreach (var bytes in frames)
            {
                BitmapSource source = Utility.CreateBitmapFromBytes(bytes);
                gEnc.Frames.Add(BitmapFrame.Create(source));
            }

            var stream = AnimationBehavior.GetSourceStream(PreviewImageControl);
            stream?.Dispose();

            var ms = new MemoryStream();
            gEnc.Save(ms);
            AnimationBehavior.SetSourceStream(PreviewImageControl, ms);
        }

        private void MainDataContext_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CurrentlySelectedFrameIndex):
                    {
                        if (CurrentlySelectedFrameIndex < 0)
                            return;

                        UpdatePreview(CurrentlySelectedFrameIndex);
                        break;
                    }
            }
        }

        private void UpdatePreview(int index)
        {
            RegeneratePreviewStream(index);
        }
    }
}

﻿using Newtonsoft.Json.Linq;
using QuanLib.BDF;
using QuanLib.Minecraft.BlockScreen;
using QuanLib.Minecraft.Datas;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.Controls
{
    /// <summary>
    /// 控件
    /// </summary>
    public abstract class Control : IComparer<Control>, IComparable<Control>
    {
        protected Control()
        {
            SubControls = new(this);
            InvokeExternalCursorMove = false;
            _ControlSyncer = null;
            ParentControl = null;
            LastRightClickTime = DateTime.MinValue;
            LastLeftClickTime = DateTime.MinValue;
            _DisplayPriority = 0;
            _MaxDisplayPriority = int.MaxValue;
            _Text = string.Empty;
            _Visible = true;
            _ClientLocation = new(0, 0);
            _ClientSize = new(MCOS.DefaultFont.HalfWidth * 4, MCOS.DefaultFont.Height);
            _AutoSize = false;
            _BorderWidth = 1;
            Skin = new(this);
            Anchor = PlaneFacing.Top | PlaneFacing.Left;
            Stretch = PlaneFacing.None;
            _ControlLayout = ContentLayout.UpperLeft;
            _ControlState = ControlState.None;

            Frame_Update = true;
            Frame_Old = null;
            Text_Update = false;
            Text_Old = Text;
            ClientLocation_Update = false;
            ClientLocation_Old = ClientLocation;
            ClientSize_Update = false;
            ClientSize_Old = ClientSize;

            CursorMove += (arg1, arg2) => { };
            CursorEnter += (arg1, arg2) => { };
            CursorLeave += (arg1, arg2) => { };
            RightClick += (obj) => { };
            LeftClick += (obj) => { };
            DoubleRightClick += (obj) => { };
            DoubleLeftClick += (obj) => { };
            TextEditorUpdate += (arg1, arg2) => { };
            BeforeFrame += Control_BeforeFrame;
            AfterFrame += Control_AfterFrame;
            InitializeCallback += () => { };
            OnAddSubControl += (obj) => { };
            OnRemoveSubControl += (obj) => { };
            OnSelected += () => { };
            OnDeselected += () => { };
            OnTextUpdate += Control_OnTextUpdate;
            OnMove += Control_OnMove;
            OnResize += Control_OnResize;
            OnTextUpdateNow += (arg1, arg2) => { };
            OnMoveNow += (arg1, arg2) => { };
            OnResizeNow += (arg1, arg2) => { };
            OnLayoutSubControl += Control_OnLayoutSubControl;
        }

        private bool Frame_Update;

        private Frame? Frame_Old;

        private bool Text_Update;

        private string Text_Old;

        private bool ClientLocation_Update;

        private Point ClientLocation_Old;

        private bool ClientSize_Update;

        private Size ClientSize_Old;

        public ControlCollection SubControls { get; }

        public bool InvokeExternalCursorMove { get; set; }

        public int Index => ParentControl?.SubControls.IndexOf(this) ?? -1;

        public ControlSyncer? ControlSyncer
        {
            get => _ControlSyncer;
            set
            {
                _ControlSyncer?.Unbinding();
                _ControlSyncer = value;
                _ControlSyncer?.Binding();
                _ControlSyncer?.Sync();
            }
        }
        private ControlSyncer? _ControlSyncer;

        public Control? ParentControl { get; protected internal set; }

        public int ParentBorderWidth => ParentControl?.BorderWidth ?? 0;

        public DateTime LastRightClickTime { get; private set; }

        public DateTime LastLeftClickTime { get; private set; }

        public string Text
        {
            get => _Text;
            set
            {
                if (_Text != value)
                {
                    string temp = _Text;
                    _Text = value;
                    Text_Update = true;
                    OnTextUpdateNow.Invoke(temp, _Text);
                    RequestUpdateFrame();
                }
            }
        }
        private string _Text;

        public bool Visible
        {
            get => _Visible;
            set
            {
                if (_Visible != value)
                {
                    _Visible = value;
                    RequestUpdateFrame();
                }
            }
        }
        private bool _Visible;

        #region 位置与尺寸

        public Point Location
        {
            get => new(ClientLocation.X + ParentBorderWidth, ClientLocation.Y + ParentBorderWidth);
            set
            {
                ClientLocation = new(value.X - ParentBorderWidth, value.Y - ParentBorderWidth);
            }
        }

        public Point ClientLocation
        {
            get => _ClientLocation;
            set
            {
                if (_ClientLocation != value)
                {
                    Point temp = _ClientLocation;
                    _ClientLocation = value;
                    ClientLocation_Update = true;
                    OnMoveNow.Invoke(temp, _ClientLocation);
                    RequestUpdateFrame();
                }
            }
        }
        private Point _ClientLocation;

        public Size ClientSize
        {
            get => _ClientSize;
            set
            {
                if (_ClientSize != value)
                {
                    Size temp = _ClientSize;
                    _ClientSize = value;
                    ClientSize_Update = true;
                    OnResizeNow.Invoke(temp, _ClientSize);
                    RequestUpdateFrame();
                }
            }
        }
        private Size _ClientSize;

        public int Width
        {
            get => ClientSize.Width + BorderWidth * 2;
            set
            {
                ClientSize = new(value - BorderWidth * 2, ClientSize.Height);
            }
        }

        public int Height
        {
            get => ClientSize.Height + BorderWidth * 2;
            set
            {
                ClientSize = new(ClientSize.Width, value - BorderWidth * 2);
            }
        }

        public int BorderWidth
        {
            get => _BorderWidth;
            set
            {
                if (_BorderWidth != value)
                {
                    _BorderWidth = value;
                    RequestUpdateFrame();
                }
            }
        }
        private int _BorderWidth;

        public int TopLocation
        {
            get => ClientLocation.Y;
            set
            {
                int offset = TopLocation - value;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
                Location = new(Location.X, Location.Y - offset);
            }
        }

        public int BottomLocation
        {
            get => ClientLocation.Y + Height - 1;
            set
            {
                int offset = value - BottomLocation;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
            }
        }

        public int LeftLocation
        {
            get => ClientLocation.X;
            set
            {
                int offset = LeftLocation - value;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
                Location = new(Location.X - offset, Location.Y);
            }
        }

        public int RightLocation
        {
            get => ClientLocation.X + Width - 1;
            set
            {
                int offset = value - RightLocation;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
            }
        }

        public int TopToBorder
        {
            get => Location.Y - ParentBorderWidth;
            set
            {
                int offset = TopToBorder - value;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
                Location = new(Location.X, Location.Y - offset);
            }
        }

        public int BottomToBorder
        {
            get => (ParentControl?.Height - ParentBorderWidth ?? GetMCOS().Screen.Height) - (Location.Y + Height);
            set
            {
                int offset = BottomToBorder - value;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
            }
        }

        public int LeftToBorder
        {
            get => Location.X - ParentBorderWidth;
            set
            {
                int offset = LeftToBorder - value;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
                Location = new(Location.X - offset, Location.Y);
            }
        }

        public int RightToBorder
        {
            get => (ParentControl?.Width - ParentBorderWidth ?? GetMCOS().Screen.Width) - (Location.X + Width);
            set
            {
                int offset = RightToBorder - value;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
            }
        }

        #endregion

        #region 外观与布局

        public ControlSkin Skin { get; }

        public bool AutoSize
        {
            get => _AutoSize;
            set
            {
                if (_AutoSize != value)
                {
                    if (value)
                        AutoSetSize();
                    _AutoSize = value;
                    RequestUpdateFrame();
                }
            }
        }
        private bool _AutoSize;

        public int DisplayPriority
        {
            get
            {
                if (IsSelected)
                    return MaxDisplayPriority;
                else
                    return _DisplayPriority;
            }
            set
            {
                _DisplayPriority = value;
                if (!IsSelected)
                    ParentControl?.SubControls.Sort();
            }
        }
        private int _DisplayPriority;

        public int MaxDisplayPriority
        {
            get => _MaxDisplayPriority;
            set
            {
                _MaxDisplayPriority = value;
                if (IsSelected)
                    ParentControl?.SubControls.Sort();
            }
        }
        private int _MaxDisplayPriority;

        /// <summary>
        /// 锚定，大小不变，位置自适应父控件
        /// </summary>
        public PlaneFacing Anchor { get; set; }

        /// <summary>
        /// 拉伸，位置不变，大小自适应父控件
        /// </summary>
        public PlaneFacing Stretch { get; set; }

        public LayoutMode LayoutMode => ControlSyncer is null ? LayoutMode.Auto : LayoutMode.Sync;

        public ContentLayout ContentLayout
        {
            get => _ControlLayout;
            set
            {
                if (_ControlLayout != value)
                {
                    _ControlLayout = value;
                    RequestUpdateFrame();
                }
            }
        }
        private ContentLayout _ControlLayout;

        public ControlState ControlState
        {
            get => _ControlState;
            set
            {
                if (_ControlState != value)
                {
                    _ControlState = value;
                    RequestUpdateFrame();
                }
            }
        }
        private ControlState _ControlState;

        public bool IsHover
        {
            get => ControlState.HasFlag(ControlState.Hover);
            private set
            {
                if (IsHover != value)
                {
                    if (value)
                    {
                        ControlState |= ControlState.Hover;
                    }
                    else
                    {
                        ControlState ^= ControlState.Hover;
                    }
                }
            }
        }

        public bool IsSelected
        {
            get => ControlState.HasFlag(ControlState.Selected);
            set
            {
                if (IsSelected != value)
                {
                    if (value)
                    {
                        ControlState |= ControlState.Selected;
                        OnSelected.Invoke();
                    }
                    else
                    {
                        ControlState ^= ControlState.Selected;
                        OnDeselected.Invoke();
                    }
                    ParentControl?.SubControls.Sort();
                }

            }
        }

        #endregion

        #region 事件声明

        public event Action<Point, CursorMode> CursorMove;

        public event Action<Point, CursorMode> CursorEnter;

        public event Action<Point, CursorMode> CursorLeave;

        public event Action<Point> RightClick;

        public event Action<Point> LeftClick;

        public event Action<Point> DoubleRightClick;

        public event Action<Point> DoubleLeftClick;

        public event Action<Point, string> TextEditorUpdate;

        public event Action BeforeFrame;

        public event Action AfterFrame;

        public event Action InitializeCallback;

        public event Action<Control> OnAddSubControl;

        public event Action<Control> OnRemoveSubControl;

        public event Action OnSelected;

        public event Action OnDeselected;

        public event Action<string, string> OnTextUpdate;

        public event Action<Point, Point> OnMove;

        public event Action<Size, Size> OnResize;

        public event Action<string, string> OnTextUpdateNow;

        public event Action<Point, Point> OnMoveNow;

        public event Action<Size, Size> OnResizeNow;

        public event Action<Size, Size> OnLayoutSubControl;

        #endregion

        #region 事件订阅

        private void Control_BeforeFrame()
        {
            if (Text_Update)
            {
                if (Text != Text_Old)
                    OnTextUpdate.Invoke(Text_Old, Text);
                Text_Update = false;
                Text_Old = Text;
            }

            if (ClientLocation_Update)
            {
                if (ClientLocation != ClientLocation_Old)
                    OnMove.Invoke(ClientLocation_Old, ClientLocation);
                ClientLocation_Update = false;
                ClientLocation_Old = ClientLocation;
            }

            if (ClientSize_Update)
            {
                if (ClientSize != ClientSize_Old)
                {
                    OnLayoutSubControl.Invoke(ClientSize_Old, ClientSize);
                    OnResize.Invoke(ClientSize_Old, ClientSize);
                }
                ClientSize_Update = false;
                ClientSize_Old = ClientSize;
            }
        }

        private void Control_AfterFrame()
        {

        }

        private void Control_OnTextUpdate(string oldText, string newText)
        {
            if (AutoSize)
                AutoSetSize();
        }

        private void Control_OnMove(Point oldPosition, Point newPosition)
        {
            if (!FormIsInitialize())
                return;
            MCOS os = GetMCOS();
            UpdateAllHover(ScreenPos2ControlPos(os.PlayerCursorReader.CurrentPosition), os.PlayerCursorReader.CursorMode);
        }

        private void Control_OnResize(Size oldSize, Size newSize)
        {
            if (!FormIsInitialize())
                return;
            MCOS os = GetMCOS();
            UpdateAllHover(ScreenPos2ControlPos(os.PlayerCursorReader.CurrentPosition), os.PlayerCursorReader.CursorMode);
        }

        private void Control_OnLayoutSubControl(Size oldSize, Size newSize)
        {
            Size offset = newSize - oldSize;
            foreach (var control in SubControls)
            {
                if (control.LayoutMode == LayoutMode.Auto)
                {
                    if (offset.Height != 0)
                    {
                        if (!control.Anchor.HasFlag(PlaneFacing.Top) && !control.Anchor.HasFlag(PlaneFacing.Bottom))
                        {
                            double proportion = (control.ClientLocation.Y + control.Height / 2.0) / oldSize.Height;
                            control.ClientLocation = new(control.ClientLocation.X, (int)Math.Round(newSize.Height * proportion - control.Height / 2.0));
                        }
                        if (control.Anchor.HasFlag(PlaneFacing.Bottom))
                            control.ClientLocation = new(control.ClientLocation.X, control.ClientLocation.Y + offset.Height);

                        if (control.Stretch.HasFlag(PlaneFacing.Top) || control.Stretch.HasFlag(PlaneFacing.Bottom))
                            control.BottomToBorder -= offset.Height;
                    }

                    if (offset.Width != 0)
                    {
                        if (!control.Anchor.HasFlag(PlaneFacing.Left) && !control.Anchor.HasFlag(PlaneFacing.Right))
                        {
                            double proportion = (control.ClientLocation.X + control.Width / 2.0) / oldSize.Width;
                            control.ClientLocation = new((int)Math.Round(newSize.Width * proportion - control.Width / 2.0), control.ClientLocation.Y);
                        }
                        if (control.Anchor.HasFlag(PlaneFacing.Right))
                            control.ClientLocation = new(control.ClientLocation.X + offset.Width, control.ClientLocation.Y);

                        if (control.Stretch.HasFlag(PlaneFacing.Left) || control.Stretch.HasFlag(PlaneFacing.Right))
                            control.RightToBorder -= offset.Width;
                    }
                }
            }
        }

        #endregion

        #region 事件处理

        internal void HandleCursorMove(Point position, CursorMode mode)
        {
            foreach (var control in SubControls.ToArray())
            {
                control.HandleCursorMove(control.ParentPos2SubPos(position), mode);
            }

            UpdateHover(position, mode);

            if (IncludedOnControl(position) || InvokeExternalCursorMove)
            {
                CursorMove.Invoke(position, mode);
            }
        }

        internal void HandleTextEditorUpdate(Point position, string text)
        {
            foreach (var control in SubControls.ToArray())
            {
                control.HandleTextEditorUpdate(control.ParentPos2SubPos(position), text);
            }

            if (Visible)
            {
                if (IncludedOnControl(position))
                    TextEditorUpdate.Invoke(position, text);
            }
        }

        internal bool HandleRightClick(Point position)
        {
            Control? control = SubControls.FirstHover;
            control?.HandleRightClick(control.ParentPos2SubPos(position));

            if (Visible)
            {
                if (IsHover)
                {
                    RightClick.Invoke(position);
                    DateTime now = DateTime.Now;
                    if (LastRightClickTime == DateTime.MinValue || (DateTime.Now - LastRightClickTime).TotalMilliseconds > 500)
                    {
                        LastRightClickTime = now;
                    }
                    else
                    {
                        DoubleRightClick.Invoke(position);
                        LastRightClickTime = DateTime.MinValue;
                    }
                    return true;
                }
            }
            return false;
        }

        internal bool HandleLeftClick(Point position)
        {
            Control? control = SubControls.FirstHover;
            control?.HandleLeftClick(control.ParentPos2SubPos(position));

            if (Visible)
            {
                if (IsHover)
                {
                    LeftClick.Invoke(position);
                    DateTime now = DateTime.Now;
                    if (LastLeftClickTime == DateTime.MinValue || (DateTime.Now - LastLeftClickTime).TotalMilliseconds > 500)
                    {
                        LastLeftClickTime = now;
                    }
                    else
                    {
                        DoubleLeftClick.Invoke(position);
                        LastLeftClickTime = DateTime.MinValue;
                    }
                    return true;
                }
            }
            return false;
        }

        internal void HandleBeforeFrame()
        {
            foreach (var control in SubControls.ToArray())
            {
                control.HandleBeforeFrame();
            }

            BeforeFrame.Invoke();
        }

        internal void HandleAfterFrame()
        {
            foreach (var control in SubControls.ToArray())
            {
                control.HandleAfterFrame();
            }

            AfterFrame.Invoke();
        }

        private void UpdateAllHover(Point position, CursorMode mode)
        {
            foreach (var control in SubControls.ToArray())
            {
                control.UpdateAllHover(control.ParentPos2SubPos(position), mode);
            }

            UpdateHover(position, mode);
        }

        private void UpdateHover(Point position, CursorMode mode)
        {
            bool included = IncludedOnControl(position);
            if (IsHover)
            {
                if (!included)
                {
                    IsHover = false;
                    CursorLeave.Invoke(position, mode);
                }
            }
            else
            {
                if (included)
                {
                    Control? control = ParentControl?.SubControls.FirstHover;
                    if (control is not null)
                    {
                        if (control.Index < Index)
                        {
                            control.IsHover = false;
                            control.CursorLeave.Invoke(position, mode);
                        }
                        else
                            return;
                    }
                    IsHover = true;
                    CursorEnter.Invoke(position, mode);
                }
            }
        }

        #endregion

        #region 位置移动

        public void ToTopMove(int offset)
        {
            Location = new(Location.X, Location.Y - offset);
        }

        public void ToBottomMove(int offset)
        {
            Location = new(Location.X, Location.Y + offset);
        }

        public void ToLeftMove(int offset)
        {
            Location = new(Location.X - offset, Location.Y);
        }

        public void ToRightMove(int offset)
        {
            Location = new(Location.X + offset, Location.Y);
        }

        public void MoveToTop(int distance)
        {
            int offset = TopToBorder - distance;
            ToTopMove(offset);
        }

        public void MoveToBottom(int distance)
        {
            int offset = BottomToBorder - distance;
            ToBottomMove(offset);
        }

        public void MoveToLeft(int distance)
        {
            int offset = LeftToBorder - distance;
            ToLeftMove(offset);
        }

        public void MoveToRight(int distance)
        {
            int offset = RightToBorder - distance;
            ToRightMove(offset);
        }

        #endregion

        #region 初始化

        public virtual void Initialize()
        {
            if (AutoSize)
                AutoSetSize();
        }

        public virtual void OnInitComplete1()
        {

        }

        public virtual void OnInitComplete2()
        {

        }

        public virtual void OnInitComplete3()
        {
            Text_Update = false;
            Text_Old = Text;
            ClientLocation_Update = false;
            ClientLocation_Old = ClientLocation;
            ClientSize_Update = false;
            ClientSize_Old = ClientSize;
        }

        internal void HandleInitialize()
        {
            Initialize();
            InitializeCallback.Invoke();
            foreach (var control in SubControls)
            {
                control.HandleInitialize();
            }
        }

        internal void HandleOnInitComplete1()
        {
            OnInitComplete1();
            foreach (var control in SubControls)
            {
                control.HandleOnInitComplete1();
            }
        }

        internal void HandleOnInitComplete2()
        {
            OnInitComplete2();
            foreach (var control in SubControls)
            {
                control.HandleOnInitComplete2();
            }
        }

        internal void HandleOnInitComplete3()
        {
            OnInitComplete3();
            foreach (var control in SubControls)
            {
                control.HandleOnInitComplete3();
            }
        }

        #endregion

        #region 帧渲染处理

        protected void RequestUpdateFrame()
        {
            Frame_Update = true;
            ParentControl?.RequestUpdateFrame();
        }

        public virtual Frame RenderingFrame()
        {
            return RenderingDefaultFrame().ToFrame();
        }

        internal Frame? RenderingAllFrame()
        {
            return UpdateSubControls(this);

            static Frame? UpdateSubControls(Control control)
            {
                Task<Frame> _task;
                if (control.Frame_Update)
                {
                    if (control.Visible)
                    {
                        _task = Task.Run(() => control.RenderingFrame());
                        _task.ContinueWith((t) =>
                        {
                            control.Frame_Update = false;
                            control.Frame_Old = t.Result;
                        });
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (control.Frame_Old is not null)
                {
                    _task = Task.Run(() => control.Frame_Old);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                if (control.SubControls.Count == 0)
                    return _task.Result;

                List<(Task<Frame?> task, Point location)> results = new();
                foreach (Control subControl in control.SubControls)
                    results.Add((Task.Run(() => UpdateSubControls(subControl)), subControl.Location));
                Task.WaitAll(results.Select(i => i.task).ToArray());
                Frame frame = _task.Result;

                foreach (var (task, location) in results)
                {
                    if (task.Result is not null)
                        frame.Overwrite(task.Result, location, new(0, 0));
                }

                return frame;
            }
        }

        protected FrameBuilder RenderingDefaultFrame()
        {
            FrameBuilder fb;
            ImageFrame? image = Skin.GetBackgroundImage();
            Frame? frame = image?.GetFrameCopy();

            int state = 0b00;
            if (!string.IsNullOrEmpty(Text))
                state += 0b01;
            if (frame is not null)
                state += 0b10;

            switch (state)
            {
                case 0b00:
                    fb = new(Frame.BuildFrame(ClientSize.Width, ClientSize.Height, Skin.GetBackgroundBlockID()));
                    break;
                case 0b01:
                    fb = RenderingText(Skin.GetForegroundBlockID(), Skin.GetBackgroundBlockID());
                    break;
                case 0b10:
                    fb = new(frame!);
                    break;
                case 0b11:
                    FrameBuilder text = RenderingText(Skin.GetForegroundBlockID(), string.Empty);
                    frame!.Overwrite(text.ToFrame(), new(0, 0), new(0, 0));
                    fb = new(frame);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            CorrectSize(fb);

            return fb;

            FrameBuilder RenderingText(string foreground, string background)
            {
                FrameBuilder result = new();
                switch (ContentLayout)
                {
                    case ContentLayout.UpperLeft:
                    case ContentLayout.LowerLeft:
                    case ContentLayout.Centered:
                        foreach (var c in Text)
                        {
                            result.AddRight(Frame.BuildFrame(MCOS.DefaultFont[c].GetBitMap(), foreground, background));
                            if (result.Width >= Width)
                                break;
                        }
                        break;
                    case ContentLayout.UpperRight:
                    case ContentLayout.LowerRight:
                        for (int i = Text.Length - 1; i >= 0; i--)
                        {
                            result.AddLeft(Frame.BuildFrame(MCOS.DefaultFont[Text[i]].GetBitMap(), foreground, background));
                            if (result.Width >= Width)
                                break;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return result;
            }
        }

        protected void CorrectSize(FrameBuilder fb)
        {
            string background = Skin.GetBackgroundBlockID();
            string border = Skin.GetBorderBlockID();

            switch (ContentLayout)
            {
                case ContentLayout.UpperLeft:
                    if (fb.Width < ClientSize.Width)
                        fb.AddRight(background, ClientSize.Width - fb.Width);
                    else if (fb.Width > ClientSize.Width)
                        fb.RemoveRight(fb.Width - ClientSize.Width);
                    if (fb.Height < ClientSize.Height)
                        fb.AddBottom(background, ClientSize.Height - fb.Height);
                    else if (fb.Height > ClientSize.Height)
                        fb.RemoveBottom(fb.Height - ClientSize.Height);
                    break;
                case ContentLayout.UpperRight:
                    if (fb.Width < ClientSize.Width)
                        fb.AddLeft(background, ClientSize.Width - fb.Width);
                    else if (fb.Width > ClientSize.Width)
                        fb.RemoveLeft(fb.Width - ClientSize.Width);
                    if (fb.Height < ClientSize.Height)
                        fb.AddBottom(background, ClientSize.Height - fb.Height);
                    else if (fb.Height > ClientSize.Height)
                        fb.RemoveBottom(fb.Height - ClientSize.Height);
                    break;
                case ContentLayout.LowerLeft:
                    if (fb.Width < ClientSize.Width)
                        fb.AddRight(background, ClientSize.Width - fb.Width);
                    else if (fb.Width > ClientSize.Width)
                        fb.RemoveRight(fb.Width - ClientSize.Width);
                    if (fb.Height < ClientSize.Height)
                        fb.AddTop(background, ClientSize.Height - fb.Height);
                    else if (fb.Height > ClientSize.Height)
                        fb.RemoveTop(fb.Height - ClientSize.Height);
                    break;
                case ContentLayout.LowerRight:
                    if (fb.Width < ClientSize.Width)
                        fb.AddLeft(background, ClientSize.Width - fb.Width);
                    else if (fb.Width > ClientSize.Width)
                        fb.RemoveLeft(fb.Width - ClientSize.Width);
                    if (fb.Height < ClientSize.Height)
                        fb.AddTop(background, ClientSize.Height - fb.Height);
                    else if (fb.Height > ClientSize.Height)
                        fb.RemoveTop(fb.Height - ClientSize.Height);
                    break;
                case ContentLayout.Centered:
                    if (fb.Width < ClientSize.Width)
                    {
                        fb.AddLeft(background, (ClientSize.Width - fb.Width) / 2);
                        fb.AddRight(background, ClientSize.Width - fb.Width);
                    }
                    else if (fb.Width > ClientSize.Width)
                    {
                        fb.RemoveLeft((fb.Width - ClientSize.Width) / 2);
                        fb.RemoveRight(fb.Width - ClientSize.Width);
                    }
                    if (fb.Height < ClientSize.Height)
                    {
                        fb.AddTop(background, (ClientSize.Height - fb.Height) / 2);
                        fb.AddBottom(background, ClientSize.Height - fb.Height);
                    }
                    else if (fb.Height > ClientSize.Height)
                    {
                        fb.RemoveTop((fb.Height - ClientSize.Height) / 2);
                        fb.RemoveBottom(fb.Height - ClientSize.Height);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            fb.AddBorder(border, BorderWidth);
        }

        #endregion

        #region 父级相关处理

        /// <summary>
        /// 顺序从根控件到子控件，不包括当前控件
        /// </summary>
        /// <returns></returns>
        public Control[] GetParentControls()
        {
            List<Control> result = new();
            Control? parent = ParentControl;
            while (parent is not null)
            {
                result.Add(parent);
                parent = parent.ParentControl;
            }
            result.Reverse();
            return result.ToArray();
        }

        public Control GetRootControl()
        {
            Control result = this;
            while (true)
            {
                Control? parent = result.ParentControl;
                if (parent is null)
                    return result;
                else
                    result = parent;
            }
        }

        public Form? GetForm()
        {
            Control? result = this;
            while (true)
            {
                if (result is null)
                    return null;
                else if (result is Form form)
                    return form;
                else
                    result = result.ParentControl;
            }
        }

        public Application GetApplication() => GetForm()?.Application ?? throw new InvalidOperationException();

        public MCOS GetMCOS() => GetApplication().MCOS;

        public Process GetProcess() => GetApplication().Process;

        public bool FormIsInitialize() => GetForm()?.IsInitialize ?? false;

        #endregion

        protected void SetTextEditorInitialText()
        {
            GetMCOS().PlayerCursorReader.InitialText = Text;
        }

        protected void ResetTextEditor()
        {
            GetMCOS().PlayerCursorReader.ResetText();
        }

        public void ClearAllControlSyncer()
        {
            ControlSyncer = null;
            foreach (var control in SubControls)
                control.ClearAllControlSyncer();
        }

        public virtual void AutoSetSize()
        {
            ClientSize = MCOS.DefaultFont.GetTotalSize(Text);
        }

        public virtual bool IncludedOnControl(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        }

        public Point ScreenPos2ControlPos(Point position)
        {
            Control[] parents = GetParentControls();
            foreach (var parent in parents)
                position = parent.ParentPos2SubPos(position);
            position = ParentPos2SubPos(position);
            return position;
        }

        public Point ParentPos2SubPos(Point position)
        {
            return new(position.X - Location.X, position.Y - Location.Y);
        }

        public Point SubPos2ParentPos(Point position)
        {
            return new(position.X + Location.X, position.Y + Location.Y);
        }

        public override string ToString()
        {
            return $"Type:{GetType().Name}|Text:{Text}|Pos:{ClientLocation.X},{ClientLocation.Y}|Size:{ClientSize.Width},{ClientSize.Height}";
        }

        public int Compare(Control? x, Control? y)
        {
            if (x?.DisplayPriority < y?.DisplayPriority)
                return -1;
            else if (x?.DisplayPriority > y?.DisplayPriority)
                return 1;
            else
                return 0;
        }

        public int CompareTo(Control? other)
        {
            return DisplayPriority.CompareTo(other?.DisplayPriority);
        }

        public class ControlSkin
        {
            public ControlSkin(Control owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));

                string black = ConcretePixel.ToBlockID(MinecraftColor.Black);
                string white = ConcretePixel.ToBlockID(MinecraftColor.White);
                string gray = ConcretePixel.ToBlockID(MinecraftColor.Gray);

                _ForegroundBlockID = black;
                _BackgroundBlockID = white;
                _BorderBlockID = gray;
                _ForegroundBlockID_Selected = black;
                _BackgroundBlockID_Selected = white;
                _BorderBlockID_Selected = gray;
                _ForegroundBlockID_Hover = black;
                _BackgroundBlockID_Hover = white;
                _BorderBlockID__Hover = gray;
                _ForegroundBlockID_Hover_Selected = black;
                _BackgroundBlockID_Hover_Selected = white;
                _BorderBlockID_Hover_Selected = gray;
                _BackgroundImage = null;
                _BackgroundImage_Hover = null;
                _BackgroundImage_Selected = null;
                _BackgroundImage_Hover_Selected = null;
            }

            private readonly Control _owner;

            public string ForegroundBlockID
            {
                get => _ForegroundBlockID;
                set
                {
                    _ForegroundBlockID = value;
                    if (_owner.ControlState == ControlState.None)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _ForegroundBlockID;

            public string BackgroundBlockID
            {
                get => _BackgroundBlockID;
                set
                {
                    _BackgroundBlockID = value;
                    if (_owner.ControlState == ControlState.None)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BackgroundBlockID;

            public string BorderBlockID
            {
                get => _BorderBlockID;
                set
                {
                    _BorderBlockID = value;
                    if (_owner.ControlState == ControlState.None)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BorderBlockID;

            public string ForegroundBlockID_Hover
            {
                get => _ForegroundBlockID_Hover;
                set
                {
                    _ForegroundBlockID_Hover = value;
                    if (_owner.ControlState == ControlState.Hover)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _ForegroundBlockID_Hover;

            public string BackgroundBlockID_Hover
            {
                get => _BackgroundBlockID_Hover;
                set
                {
                    _BackgroundBlockID_Hover = value;
                    if (_owner.ControlState == ControlState.Hover)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BackgroundBlockID_Hover;

            public string BorderBlockID__Hover
            {
                get => _BorderBlockID__Hover;
                set
                {
                    _BorderBlockID__Hover = value;
                    if (_owner.ControlState == ControlState.Hover)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BorderBlockID__Hover;

            public string ForegroundBlockID_Selected
            {
                get => _ForegroundBlockID_Selected;
                set
                {
                    _ForegroundBlockID_Selected = value;
                    if (_owner.ControlState == ControlState.Selected)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _ForegroundBlockID_Selected;

            public string BackgroundBlockID_Selected
            {
                get => _BackgroundBlockID_Selected;
                set
                {
                    _BackgroundBlockID_Selected = value;
                    if (_owner.ControlState == ControlState.Selected)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BackgroundBlockID_Selected;

            public string BorderBlockID_Selected
            {
                get => _BorderBlockID_Selected;
                set
                {
                    _BorderBlockID_Selected = value;
                    if (_owner.ControlState == ControlState.Selected)
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BorderBlockID_Selected;

            public string ForegroundBlockID_Hover_Selected
            {
                get => _ForegroundBlockID_Hover_Selected;
                set
                {
                    _ForegroundBlockID_Hover_Selected = value;
                    if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
                        _owner.RequestUpdateFrame();
                }
            }
            private string _ForegroundBlockID_Hover_Selected;

            public string BackgroundBlockID_Hover_Selected
            {
                get => _BackgroundBlockID_Hover_Selected;
                set
                {
                    _BackgroundBlockID_Hover_Selected = value;
                    if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BackgroundBlockID_Hover_Selected;

            public string BorderBlockID_Hover_Selected
            {
                get => _BorderBlockID_Hover_Selected;
                set
                {
                    _BorderBlockID_Hover_Selected = value;
                    if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
                        _owner.RequestUpdateFrame();
                }
            }
            private string _BorderBlockID_Hover_Selected;

            public ImageFrame? BackgroundImage
            {
                get => _BackgroundImage;
                set
                {
                    _BackgroundImage = value;
                    if (_owner.ControlState == ControlState.None)
                        _owner.RequestUpdateFrame();
                }
            }
            private ImageFrame? _BackgroundImage;

            public ImageFrame? BackgroundImage_Hover
            {
                get => _BackgroundImage_Hover;
                set
                {
                    _BackgroundImage_Hover = value;
                    if (_owner.ControlState == ControlState.Hover)
                        _owner.RequestUpdateFrame();
                }
            }
            private ImageFrame? _BackgroundImage_Hover;

            public ImageFrame? BackgroundImage_Selected
            {
                get => _BackgroundImage_Selected;
                set
                {
                    _BackgroundImage_Selected = value;
                    if (_owner.ControlState == ControlState.Selected)
                        _owner.RequestUpdateFrame();
                }
            }
            public ImageFrame? _BackgroundImage_Selected;

            public ImageFrame? BackgroundImage_Hover_Selected
            {
                get => _BackgroundImage_Hover_Selected;
                set
                {
                    _BackgroundImage_Hover_Selected = value;
                    if (_owner.ControlState == (ControlState.Hover | ControlState.Selected))
                        _owner.RequestUpdateFrame();
                }
            }
            private ImageFrame? _BackgroundImage_Hover_Selected;

            public string GetForegroundBlockID()
            {
                return _owner.ControlState switch
                {
                    ControlState.None => ForegroundBlockID,
                    ControlState.Hover => ForegroundBlockID_Hover,
                    ControlState.Selected => ForegroundBlockID_Selected,
                    ControlState.Hover | ControlState.Selected => ForegroundBlockID_Hover_Selected,
                    _ => throw new InvalidOperationException(),
                };
            }

            public string GetBackgroundBlockID()
            {
                return _owner.ControlState switch
                {
                    ControlState.None => BackgroundBlockID,
                    ControlState.Hover => BackgroundBlockID_Hover,
                    ControlState.Selected => BackgroundBlockID_Selected,
                    ControlState.Hover | ControlState.Selected => BackgroundBlockID_Hover_Selected,
                    _ => throw new InvalidOperationException(),
                };
            }

            public string GetBorderBlockID()
            {
                return _owner.ControlState switch
                {
                    ControlState.None => BorderBlockID,
                    ControlState.Hover => BorderBlockID__Hover,
                    ControlState.Selected => BorderBlockID_Selected,
                    ControlState.Hover | ControlState.Selected => BorderBlockID_Hover_Selected,
                    _ => throw new InvalidOperationException(),
                };
            }

            public ImageFrame? GetBackgroundImage()
            {
                return _owner.ControlState switch
                {
                    ControlState.None => BackgroundImage,
                    ControlState.Hover => BackgroundImage_Hover,
                    ControlState.Selected => BackgroundImage_Selected,
                    ControlState.Hover | ControlState.Selected => BackgroundImage_Hover_Selected,
                    _ => throw new InvalidOperationException(),
                };
            }

            public void SetAllForegroundBlockID(string blockID)
            {
                ForegroundBlockID = blockID;
                ForegroundBlockID_Hover = blockID;
                ForegroundBlockID_Selected = blockID;
                ForegroundBlockID_Hover_Selected = blockID;
            }

            public void SetAllBackgroundBlockID(string blockID)
            {
                BackgroundBlockID = blockID;
                BackgroundBlockID_Hover = blockID;
                BackgroundBlockID_Selected = blockID;
                BackgroundBlockID_Hover_Selected = blockID;
            }

            public void SetAllBorderBlockID(string blockID)
            {
                BorderBlockID = blockID;
                BorderBlockID__Hover = blockID;
                BorderBlockID_Selected = blockID;
                BorderBlockID_Hover_Selected = blockID;
            }

            public void SetAllBackgroundImage(ImageFrame? frame)
            {
                BackgroundImage = frame;
                BackgroundImage_Hover = frame;
                BackgroundImage_Selected = frame;
                BackgroundImage_Hover_Selected = frame;
            }

            public void SetForegroundBlockID(ControlState state, string blockID)
            {
                switch (state)
                {
                    case ControlState.None:
                        ForegroundBlockID = blockID;
                        break;
                    case ControlState.Hover:
                        ForegroundBlockID_Hover = blockID;
                        break;
                    case ControlState.Selected:
                        ForegroundBlockID_Selected = blockID;
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        ForegroundBlockID_Hover_Selected = blockID;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void SetBackgroundBlockID(ControlState state, string blockID)
            {
                switch (state)
                {
                    case ControlState.None:
                        BackgroundBlockID = blockID;
                        break;
                    case ControlState.Hover:
                        BackgroundBlockID_Hover = blockID;
                        break;
                    case ControlState.Selected:
                        BackgroundBlockID_Selected = blockID;
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        BackgroundBlockID_Hover_Selected = blockID;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void SetBorderBlockID(ControlState state, string blockID)
            {
                switch (state)
                {
                    case ControlState.None:
                        BorderBlockID = blockID;
                        break;
                    case ControlState.Hover:
                        BorderBlockID__Hover = blockID;
                        break;
                    case ControlState.Selected:
                        BorderBlockID_Selected = blockID;
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        BorderBlockID_Hover_Selected = blockID;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void SetBackgroundImage(ControlState state, ImageFrame? frame)
            {
                switch (state)
                {
                    case ControlState.None:
                        BackgroundImage = frame;
                        break;
                    case ControlState.Hover:
                        BackgroundImage_Hover = frame;
                        break;
                    case ControlState.Selected:
                        BackgroundImage_Selected = frame;
                        break;
                    case ControlState.Hover | ControlState.Selected:
                        BackgroundImage_Hover_Selected = frame;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public class ControlCollection : IList<Control>, IReadOnlyList<Control>
        {
            public ControlCollection(Control owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
                _items = new();
            }

            private readonly Control _owner;

            private readonly List<Control> _items;

            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public bool HaveHover => FirstHover is not null;

            public Control? FirstHover
            {
                get
                {
                    for (int i = _items.Count - 1; i >= 0; i--)
                    {
                        if (_items[i].IsHover)
                            return _items[i];
                    }
                    return null;
                }
            }

            public bool HaveSelected => FirstSelected is not null;

            public Control? FirstSelected
            {
                get
                {
                    for (int i = _items.Count - 1; i >= 0; i--)
                    {
                        if (_items[i].IsSelected)
                            return _items[i];
                    }
                    return null;
                }
            }

            public Control? RecentlyAddedControl { get; private set; }

            public Control? RecentlyRemovedControl { get; private set; }

            Control IList<Control>.this[int index] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public Control this[int index] => _items[index];

            public void ClearSelected()
            {
                foreach (Control control in _items.ToArray())
                    control.IsSelected = false;
            }

            public void Add(Control item)
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                item.ParentControl = _owner;
                item.OnSelected += Sort;
                item.OnDeselected += Sort;

                bool insert = false;
                for (int i = _items.Count - 1; i >= 0; i--)
                {
                    if (item.DisplayPriority >= _items[i].DisplayPriority)
                    {
                        _items.Insert(i + 1, item);
                        insert = true;
                        break;
                    }
                }
                if (!insert)
                    _items.Insert(0, item);
                RecentlyAddedControl = item;
                _owner.OnAddSubControl.Invoke(item);
                _owner.RequestUpdateFrame();
            }

            public bool TryAdd(Control item)
            {
                if (_items.Contains(item))
                {
                    return false;
                }
                else
                {
                    Add(item);
                    return true;
                }
            }

            void IList<Control>.Insert(int index, Control item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(Control item)
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                item.ParentControl = null;
                item.OnSelected -= Sort;
                item.OnDeselected -= Sort;
                bool result = _items.Remove(item);
                RecentlyRemovedControl = item;
                _owner.OnAddSubControl.Invoke(item);
                _owner.RequestUpdateFrame();

                return result;
            }

            public void RemoveAt(int index)
            {
                _items.Remove(_items[index]);
            }

            public void Clear()
            {
                foreach (var item in _items.ToArray())
                    Remove(item);
            }

            public void Sort()
            {
                _items.Sort();
            }

            public bool Contains(Control item)
            {
                return _items.Contains(item);
            }

            public int IndexOf(Control item)
            {
                return _items.IndexOf(item);
            }

            public Control[] GetHovers()
            {
                List<Control> result = new();
                foreach (var item in _items)
                    if (item.IsHover)
                        result.Add(item);
                return result.ToArray();
            }

            public Control[] GetSelecteds()
            {
                List<Control> result = new();
                foreach (var item in _items)
                    if (item.IsSelected)
                        result.Add(item);
                return result.ToArray();
            }

            public Control[] ToArray()
            {
                return _items.ToArray();
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public IEnumerator<Control> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_items).GetEnumerator();
            }
        }
    }
}
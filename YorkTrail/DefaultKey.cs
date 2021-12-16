using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YorkTrail
{
    public enum CommandName
    {
        Play,
        Pause,
        Stop,
        FF,
        FR,
        ToPrevMarker,
        ToNextMarker,
        CurrentToStartPosition,
        CurrentToEndPosition,
        Stereo,
        Mono,
        LOnly,
        ROnly,
        LMinusR,
        LpfOn,
        HpfOn,
        BpfOn,
        PitchQuad,
        PitchDouble,
        PitchNormal,
        PitchHalf,
        TempoDouble,
        TempoNormal,
        TempoHalf,
        TempoOneThird,
        TempoQuarter,
        Bypass,
        FileOpen,
        FileClose,
        Loop,
        SelectionReset,
        ZoomIn,
        ZoomOut,
        OpenTempoCalcWindow,
        AlwaysOnTop,
        ShowTimeAtMeasure,
        SnapToTick,
        Exit,
        LinkSliders,
        AddMarker,
        ClearMarker,
    }

    public class KeyCommand
    {
        public KeyCommand(string name, ShortCutKey key, ICommand cmd, object obj = null)
        {
            LocalName = name;
            ShortCutKey = key;
            Command = cmd;
            CommandParameter = obj;
        }
        public string LocalName { get; set; }
        public ShortCutKey ShortCutKey { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
    }

    public static class DefaultKey
    {
        public static readonly ReadOnlyDictionary<CommandName, KeyCommand> KeyCommands = new ReadOnlyDictionary<CommandName, KeyCommand>(new Dictionary<CommandName, KeyCommand>
        {
            { CommandName.Play, new KeyCommand("再生", new ShortCutKey(Key.Space, ModifierKeys.None), CommandCollection.Get(nameof(PlayCommand))) },
            { CommandName.Pause, new KeyCommand("一時停止", new ShortCutKey(Key.Enter, ModifierKeys.None), CommandCollection.Get(nameof(PauseCommand))) },
            { CommandName.Stop, new KeyCommand("停止", new ShortCutKey(Key.Escape, ModifierKeys.None), CommandCollection.Get(nameof(StopCommand))) },
            { CommandName.FF, new KeyCommand("早送り", new ShortCutKey(Key.Right, ModifierKeys.None), CommandCollection.Get(nameof(FFCommand))) },
            { CommandName.FR, new KeyCommand("巻き戻し", new ShortCutKey(Key.Left, ModifierKeys.None), CommandCollection.Get(nameof(FRCommand))) },
            { CommandName.ToPrevMarker, new KeyCommand("前のマーカーへ", new ShortCutKey(Key.Left, ModifierKeys.Shift), CommandCollection.Get(nameof(ToPrevMarkerCommand))) },
            { CommandName.ToNextMarker, new KeyCommand("次のマーカーへ", new ShortCutKey(Key.Right, ModifierKeys.Shift), CommandCollection.Get(nameof(ToNextMarkerCommand))) },
            { CommandName.CurrentToStartPosition, new KeyCommand("現在位置を開始位置に", new ShortCutKey(Key.F9, ModifierKeys.None), CommandCollection.Get(nameof(CurrentToStartPositionCommand))) },
            { CommandName.CurrentToEndPosition, new KeyCommand("現在位置を終了位置に", new ShortCutKey(Key.F10, ModifierKeys.None), CommandCollection.Get(nameof(CurrentToEndPositionCommand))) },
            { CommandName.Stereo, new KeyCommand("ステレオ", new ShortCutKey(Key.S, ModifierKeys.None), CommandCollection.Get(nameof(ChannelCommand)), Channels.Stereo) },
            { CommandName.Mono, new KeyCommand("左ch + 右ch", new ShortCutKey(Key.M, ModifierKeys.None), CommandCollection.Get(nameof(ChannelCommand)), Channels.Mono) },
            { CommandName.LOnly, new KeyCommand("左chのみ", new ShortCutKey(Key.L, ModifierKeys.None), CommandCollection.Get(nameof(ChannelCommand)), Channels.LOnly) },
            { CommandName.ROnly, new KeyCommand("右chのみ", new ShortCutKey(Key.R, ModifierKeys.None), CommandCollection.Get(nameof(ChannelCommand)), Channels.ROnly) },
            { CommandName.LMinusR, new KeyCommand("左ch - 右ch", new ShortCutKey(Key.M, ModifierKeys.Shift), CommandCollection.Get(nameof(ChannelCommand)), Channels.LMinusR) },
            { CommandName.LpfOn, new KeyCommand("LPF有効", new ShortCutKey(Key.D0, ModifierKeys.None), CommandCollection.Get(nameof(LpfOnCommand))) },
            { CommandName.HpfOn, new KeyCommand("HPF有効", new ShortCutKey(Key.D1, ModifierKeys.None), CommandCollection.Get(nameof(HpfOnCommand))) },
            { CommandName.BpfOn, new KeyCommand("BPF有効", new ShortCutKey(Key.D2, ModifierKeys.None), CommandCollection.Get(nameof(BpfOnCommand))) },
            { CommandName.PitchQuad, new KeyCommand("ピッチ2オクターブ上げ", new ShortCutKey(Key.F2, ModifierKeys.Shift), CommandCollection.Get(nameof(PitchCommand)), 4.0f) },
            { CommandName.PitchDouble, new KeyCommand("ピッチ1オクターブ上げ", new ShortCutKey(Key.F2, ModifierKeys.None), CommandCollection.Get(nameof(PitchCommand)), 2.0f) },
            { CommandName.PitchNormal, new KeyCommand("ピッチ通常", new ShortCutKey(Key.F3, ModifierKeys.None), CommandCollection.Get(nameof(PitchCommand)), 1.0f) },
            { CommandName.PitchHalf, new KeyCommand("ピッチ1オクターブ下げ", new ShortCutKey(Key.F4, ModifierKeys.None), CommandCollection.Get(nameof(PitchCommand)), 0.5f) },
            { CommandName.TempoDouble, new KeyCommand("テンポ2倍", new ShortCutKey(Key.F5, ModifierKeys.None), CommandCollection.Get(nameof(TempoCommand)), 2.0f) },
            { CommandName.TempoNormal, new KeyCommand("テンポ通常", new ShortCutKey(Key.F6, ModifierKeys.None), CommandCollection.Get(nameof(TempoCommand)), 1.0f) },
            { CommandName.TempoHalf, new KeyCommand("テンポ1/2", new ShortCutKey(Key.F7, ModifierKeys.None), CommandCollection.Get(nameof(TempoCommand)), 0.5f) },
            { CommandName.TempoOneThird, new KeyCommand("テンポ1/3", new ShortCutKey(Key.F8, ModifierKeys.None), CommandCollection.Get(nameof(TempoCommand)), 0.33f) },
            { CommandName.TempoQuarter, new KeyCommand("テンポ1/4", new ShortCutKey(Key.F8, ModifierKeys.Shift), CommandCollection.Get(nameof(TempoCommand)), 0.25f) },
            { CommandName.Bypass, new KeyCommand("バイパス", new ShortCutKey(Key.B, ModifierKeys.None), CommandCollection.Get(nameof(BypassCommand))) },
            { CommandName.FileOpen, new KeyCommand("ファイルを開く", new ShortCutKey(Key.O, ModifierKeys.Control), CommandCollection.Get(nameof(FileOpenCommand))) },
            { CommandName.FileClose, new KeyCommand("ファイルを閉じる", new ShortCutKey(Key.None, ModifierKeys.None), CommandCollection.Get(nameof(FileCloseCommand))) },
            { CommandName.Loop, new KeyCommand("ループ再生", new ShortCutKey(Key.L, ModifierKeys.Control), CommandCollection.Get(nameof(LoopCommand))) },
            { CommandName.SelectionReset, new KeyCommand("再生区間リセット", new ShortCutKey(Key.I, ModifierKeys.Control), CommandCollection.Get(nameof(SelectionResetCommand))) },
            { CommandName.ZoomIn, new KeyCommand("再生区間拡大", new ShortCutKey(Key.X, ModifierKeys.Control), CommandCollection.Get(nameof(ZoomInCommand))) },
            { CommandName.ZoomOut, new KeyCommand("再生区間縮小", new ShortCutKey(Key.Z, ModifierKeys.Control), CommandCollection.Get(nameof(ZoomOutCommand))) },
            { CommandName.OpenTempoCalcWindow, new KeyCommand("テンポ計算ウィンドウ", new ShortCutKey(Key.T, ModifierKeys.Alt), CommandCollection.Get(nameof(OpenTempoCalcWindowCommand))) },
            { CommandName.AlwaysOnTop, new KeyCommand("常に手前に表示", new ShortCutKey(Key.T, ModifierKeys.Control), CommandCollection.Get(nameof(AlwaysOnTopCommand))) },
            { CommandName.ShowTimeAtMeasure, new KeyCommand("小節で表示", new ShortCutKey(Key.None, ModifierKeys.None), CommandCollection.Get(nameof(ShowTimeAtMeasureCommand))) },
            { CommandName.SnapToTick, new KeyCommand("ティックにスナップ", new ShortCutKey(Key.None, ModifierKeys.None), CommandCollection.Get(nameof(SnapToTickCommand))) },
            { CommandName.Exit, new KeyCommand("終了", new ShortCutKey(Key.Q, ModifierKeys.Control), CommandCollection.Get(nameof(ExitCommand))) },
            { CommandName.LinkSliders, new KeyCommand("再生区間を固定", new ShortCutKey(Key.F, ModifierKeys.Control), CommandCollection.Get(nameof(LinkSlidersCommand))) },
            { CommandName.AddMarker, new KeyCommand("マーカーを追加", new ShortCutKey(Key.M, ModifierKeys.Control), CommandCollection.Get(nameof(AddMarkerCommand))) },
            { CommandName.ClearMarker, new KeyCommand("マーカーをクリア", new ShortCutKey(Key.M, ModifierKeys.Control | ModifierKeys.Shift), CommandCollection.Get(nameof(ClearMarkerCommand))) },
        });

        public static ICommand GetCommand(CommandName cmd)
        {
            return KeyCommands[cmd].Command;
        }

        public static Dictionary<CommandName, ShortCutKey> GetKeyBinds()
        {
            var dic = new Dictionary<CommandName, ShortCutKey>();

            foreach (var kc in KeyCommands)
            {
                dic.Add(kc.Key, kc.Value.ShortCutKey);
            }

            return dic;
        }
    }
}

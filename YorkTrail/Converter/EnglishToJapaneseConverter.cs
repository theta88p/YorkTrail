using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace YorkTrail
{
    class EnglishToJapaneseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Play": return "再生";
                case "Pause": return "一時停止";
                case "Stop": return "停止";
                case "FF": return "早送り";
                case "FR": return "巻き戻し";
                case "ToStart": return "先頭に移動";
                case "ToEnd": return "最後に移動";
                case "CurrentToStartPosition": return "現在位置を開始位置に";
                case "CurrentToEndPosition": return "現在位置を終了位置に";
                case "Stereo": return "ステレオ";
                case "Mono": return "左ch + 右ch";
                case "LOnly": return "左chのみ";
                case "ROnly": return "右chのみ";
                case "LMinusR": return "左ch - 右ch";
                case "LpfOn": return "LPF有効";
                case "HpfOn": return "HPF有効";
                case "BpfOn": return "BPF有効";
                case "PitchQuad": return "ピッチ2オクターブ上げ";
                case "PitchDouble": return "ピッチ1オクターブ上げ";
                case "PitchNormal": return "ピッチ通常";
                case "PitchHalf": return "ピッチ1オクターブ下げ";
                case "TempoDouble": return "テンポ2倍";
                case "TempoNormal": return "テンポ通常";
                case "TempoHalf": return "テンポ1/2";
                case "TempoOneThird": return "テンポ1/3";
                case "TempoQuarter": return "テンポ1/4";
                case "Bypass": return "バイパス";
                case "FileOpen": return "ファイルを開く";
                case "FileClose": return "ファイルを閉じる";
                case "Loop": return "ループ再生";
                case "SelectionReset": return "再生区間リセット";
                case "Zoom": return "再生位置拡大/縮小";
                case "OpenTempoCalcWindow": return "テンポ計算ウィンドウ";
                case "AlwaysOnTop": return "常に手前に表示";
                case "Exit": return "終了";
                default: return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }
    }
}

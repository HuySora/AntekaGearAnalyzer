using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tesseract;

namespace AntekaEquipmentAnalyzer {
    public partial class Analyzer : Form {
        internal HookProc _globalLlMouseHookCallback;
        internal IntPtr _hGlobalLlMouseHook;
        private static IntPtr _targetWindow;
        public static bool _ignoreResolution = false;

        public Analyzer() {
            InitializeComponent();
            groupBox_Substat.Dispose();
        }

        #region Window Selection

        //Pinvoke shit
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out Point lpPoint);
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        // When you click this button, it'll hook the mouse event globally for the next press.
        private void button_SelectWindow_Click(object sender, EventArgs e) {
            // Create an instance of HookProc.
            _globalLlMouseHookCallback = SelectWindow;

            _hGlobalLlMouseHook = NativeMethods.SetWindowsHookEx(
                HookType.WH_MOUSE_LL,
                _globalLlMouseHookCallback,
                Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
                0);
        }
        public int SelectWindow(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                // Get the mouse WM from the wParam parameter
                var wmMouse = (MouseMessage)wParam;
                if (wmMouse == MouseMessage.WM_LBUTTONDOWN) {
                    Point p;
                    if (GetCursorPos(out p)) {
                        _targetWindow = WindowFromPoint(p);
                        int length = GetWindowTextLength(_targetWindow);
                        StringBuilder sb = new StringBuilder(length + 1);
                        GetWindowText(_targetWindow, sb, sb.Capacity);
                        textBox_WindowSelected.Text = sb.ToString();
                    }

                    if (_hGlobalLlMouseHook != IntPtr.Zero) {
                        // Unhook the low-level mouse hook
                        if (!NativeMethods.UnhookWindowsHookEx(_hGlobalLlMouseHook))
                            throw new Win32Exception("Unable to clear hook;");
                        _hGlobalLlMouseHook = IntPtr.Zero;
                    }
                }
            }

            // Pass the hook information to the next hook procedure in chain
            return NativeMethods.CallNextHookEx(_hGlobalLlMouseHook, nCode, wParam, lParam);
        }
        #endregion

        #region Screenshot Capture

        //Pinvoke shit
        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr handle, ref Rectangle rect);
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr handle, out Rectangle rect);

        public static Bitmap CropPercent(Bitmap b, float left, float right, float top, float bottom) {
            var height = b.Height * (1f - top - bottom);
            var width = b.Width * (1f - left - right);
            Bitmap nb = new Bitmap((int)width, (int)height);
            using (Graphics g = Graphics.FromImage(nb)) {
                g.DrawImage(b, -b.Width * left, -b.Height * top);
                return nb;
            }
        }

        static Bitmap TrimBitmap(Bitmap source) {
            Rectangle srcRect = default(Rectangle);
            BitmapData data = null;
            try {
                data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte[] buffer = new byte[data.Height * data.Stride];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                int xMin = int.MaxValue;
                int xMax = 0;
                int yMin = int.MaxValue;
                int yMax = 0;
                for (int y = 0; y < data.Height; y++) {
                    for (int x = 0; x < data.Width; x++) {
                        int totalVal = 0;
                        for (int i = 0; i < 3; i++)
                            totalVal += buffer[y * data.Stride + 4 * x + i];
                        if (totalVal < 765) {
                            if (x < xMin)
                                xMin = x;
                            if (x > xMax)
                                xMax = x;
                            if (y < yMin)
                                yMin = y;
                            if (y > yMax)
                                yMax = y;
                        }
                    }
                }
                if (xMax < xMin || yMax < yMin) {
                    // Image is empty...
                    return null;
                }
                srcRect = Rectangle.FromLTRB(xMin, yMin + 36, xMax, yMax - 53);
            } finally {
                if (data != null)
                    source.UnlockBits(data);
            }

            Bitmap dest = new Bitmap(srcRect.Width, srcRect.Height);
            Rectangle destRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
            using (Graphics graphics = Graphics.FromImage(dest)) {
                graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
            }
            return dest;
        }

        // We might be able to avoid a lot of trimming/time if we capture only the client area, but
        // In my tests bluestacks was still pulling a blank top so who knows for sure.
        static Bitmap CaptureImage() {
            Rectangle rect = new Rectangle();
            GetClientRect(_targetWindow, out rect);

            //rect.Width = rect.Width - rect.X;
            //rect.Height = rect.Height - rect.Y;

            // Create a bitmap to draw the capture into
            using (Bitmap bitmap = new Bitmap(rect.Width, rect.Height)) {
                // Use PrintWindow to draw the window into our bitmap
                using (Graphics g = Graphics.FromImage(bitmap)) {
                    IntPtr hdc = g.GetHdc();
                    if (!PrintWindow(_targetWindow, hdc, 0x00000002)) {
                        int error = Marshal.GetLastWin32Error();
                        var exception = new System.ComponentModel.Win32Exception(error);
                        Debug.WriteLine("ERROR: " + error + ": " + exception.Message);
                        // TODO: Throw the exception?
                    }
                    g.ReleaseHdc(hdc);
                }
                Directory.CreateDirectory("images");
                var trimmed = TrimBitmap(bitmap);
                bitmap.Save("images/raw_screen.png");
                trimmed.Save("images/raw_screen_trimmed.png");
                return trimmed;
            }
        }

        // We're going to check the average brightness of each pixel and set it to black or white based on
        // a cut off threshold.
        public static Bitmap Polarize(Bitmap bmp, float cutoff, bool forceBlack = true, int maxColorDist = 255, bool invert = false) {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            for (int counter = 0; counter < rgbValues.Length; counter++) {
                //scan over rgb
                var brightness = 0f;
                var minBrightness = 255;
                var minColor = 255;
                var maxColor = 0;
                for (int i = 0; i < 3; i++) {
                    brightness += rgbValues[counter + i];
                    if (rgbValues[counter + i] < minBrightness)
                        minBrightness = rgbValues[counter + i];
                    if (rgbValues[counter + i] < minColor)
                        minColor = rgbValues[counter + i];
                    if (rgbValues[counter + i] > maxColor)
                        maxColor = rgbValues[counter + i];
                }
                brightness /= (255 * 3);

                for (int i = 0; i < 3; i++) {
                    if (maxColor - minColor > maxColorDist)
                        rgbValues[counter + i] = 255;
                    else
                        rgbValues[counter + i] = (byte)(brightness > cutoff ? (forceBlack ? 0 : (invert ? brightness : 1 - brightness) * 255) : 255);
                }
                counter += 3;
            }
            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public bool CropImage() {
            Bitmap bp = (Bitmap)Bitmap.FromFile("images/raw_screen_trimmed.png");

            // These crops should be able to be set by selection, but fuck it for now.
            // Hard code baby lets go.

            // This is the stats - I'm going to save these seperately in case I need to debug
            var cropped = CropPercent(bp, 0.02f, 0.71f, 0.37f, 0.47f);
            cropped.Save("images/stats.png");
            Polarize(cropped, 0.2f, false, 20, false).Save("images/stats_polarized.png");
            cropped = CropPercent(bp, 0.02f, 0.71f, 0.37f, 0.47f);
            Polarize(cropped, 0.2f, false, 20, true).Save("images/stats_polarized_inverted.png");

            // This is the gear level bubble.
            var gearLevel = CropPercent(bp, 0.074f, 0.9f, 0.11f, 0.85f);
            gearLevel.Save("images/gearlevel.png");
            Polarize(gearLevel, 0.8f, false, 20).Save("images/gearlevel_polarized.png");

            // This is the gear type
            var gearType = CropPercent(bp, 0.105f, 0.82f, 0.1f, 0.82f);
            gearType.Save("images/geartype.png");
            Polarize(gearType, 0.3f).Save("images/geartype_polarized.png");

            var tooSmall = bp.Width < 1000;
            bp.Dispose();
            if (tooSmall && !_ignoreResolution) {
                Analyzer._ignoreResolution = true;
                return !(MessageBox.Show("The size of the captured image is smaller than the recommended, this may result in errors if you proceed anyway. You can increase the size of your emulator window to get better results. This error will only display once. Proceed anyway?", "Resolution Size Issue", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No);
            }
            return true;
        }


        #endregion

        private void button_Check_Click(object sender, EventArgs e) {
            if (_targetWindow != IntPtr.Zero)
                CaptureImage(); //Saved image
            if (!File.Exists("images/raw_screen_trimmed.png")) {
                MessageBox.Show("No image to process exists. Make sure you're selecting the correct window.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                if (!CropImage()) {
                    return;
                }
                try {
                    AnalyzeGear();
                } catch {
                    MessageBox.Show("There was an error analyzing gear. You can try resizing the emulator to potentially fix the issue. If you'd like to report the error, please add the image located in 'AntekaEquipmentAnalyzer\\images\\raw_screen_trimmed.png' to an issue on the github page.", "Error", MessageBoxButtons.OK);
                };
            }
        }

        private void AnalyzeGear() {
            foreach (var c in flowLayoutPanel_Substats.Controls)
                ((GroupBox)c).Dispose();
            flowLayoutPanel_Substats.Controls.Clear();

            string sGearStats, sGearLevel, sGearType, sGearStatsInverted;
            // First we need to OCR all the images that have been cut to build the item.
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default)) {
                engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                Bitmap bmp = (Bitmap)Bitmap.FromFile("images/stats_polarized.png");
                using (var img = PixConverter.ToPix(bmp))
                using (var page = engine.Process(img))
                    sGearStats = page.GetText();

                bmp.Dispose();
                bmp = (Bitmap)Bitmap.FromFile("images/stats_polarized_inverted.png");
                using (var img = PixConverter.ToPix(bmp))
                using (var page = engine.Process(img))
                    sGearStatsInverted = page.GetText();

                bmp.Dispose();
                bmp = (Bitmap)Bitmap.FromFile("images/gearlevel_polarized.png");
                using (var img = PixConverter.ToPix(bmp))
                using (var page = engine.Process(img))
                    sGearLevel = page.GetText();

                bmp.Dispose();
                bmp = (Bitmap)Bitmap.FromFile("images/geartype_polarized.png");
                using (var img = PixConverter.ToPix(bmp))
                using (var page = engine.Process(img))
                    sGearType = page.GetText();
                bmp.Dispose();
            }
            // Build the gear piece
            var gear = new Gear();
            gear.SetGearEnhanceFromString(sGearLevel);
            gear.SetGearTypeFromString(sGearType);

            // Build the substats array
            var sGearStatsToks = Regex.Replace(sGearStats, @"\t|\n|\r", "|").Split('|').Where(x => x != string.Empty).ToArray();
            var sGearStatsInvToks = Regex.Replace(sGearStatsInverted, @"\t|\n|\r", "|").Split('|').Where(x => x != string.Empty).ToArray();
            var sGearStatsCombined = new List<string>();
            for (int i = 0; i < Math.Max(sGearStatsToks.Length, sGearStatsInvToks.Length); i++) {
                // If one has digits and the other does not, just add that one
                // If they both have digits, use the longer one for now
                if (!sGearStatsToks[i].Any(char.IsDigit) && sGearStatsInvToks[i].Any(char.IsDigit))
                    sGearStatsCombined.Add(sGearStatsInvToks[i]);
                else if (!sGearStatsInvToks[i].Any(char.IsDigit) && sGearStatsToks[i].Any(char.IsDigit))
                    sGearStatsCombined.Add(sGearStatsToks[i]);
                else
                    sGearStatsCombined.Add(sGearStatsInvToks[i].Length > sGearStatsToks[i].Length ? sGearStatsInvToks[i] : sGearStatsToks[i]);
            }
            gear.AddSubstatsFromString(sGearStatsCombined.ToArray());
            gear.AttemptToAssignRollCounts(); // Try to figure out where things rolled
            gear.CalculateIdealRolls();

            foreach (var sub in gear.subs)
                flowLayoutPanel_Substats.Controls.Add(new SubstatInfo(sub, gear.gearType).groupBox_Substat);

            label_GearScore.Text = $"{gear.gearscore:0.00}";
            label_GearScoreReforged.Text = $"{gear.gearscoreReforge:0.00}";
            label_MaxPotential.Text = $"{(gear.gearscoreReforge + gear.idealIncrease):0.00}";
            textBox_Enhancement.Text = $"+{gear.eLevel}";
            textBox_Quality.Text = $"{gear.gearTypeStr}";

            // Calculate weighted percent total
            var maxPossibleGearscore = gear.subs.Sum(x => x.maxPossibleGearScoreValue(gear.gearType));
            var minPossibleGearscore = gear.subs.Sum(x => x.minPossibleGearScoreValue(gear.gearType));
            var weightedTotal = (gear.gearscore - minPossibleGearscore) / (maxPossibleGearscore - minPossibleGearscore) * 100;
            progressBar_WeightedTotal.Value = (int)weightedTotal;
            label_WeightedTotal.Text = $"{(int)weightedTotal}%";
        }

    }

    public class Sub_CritDamage : Substat {
        public override string name => "Crit Damage";
        public Sub_CritDamage(int val) : base(val) { }
        public override float scoreMulti => 8f / 7f;
        public override int[] maxRoll => new[] { 7, 7 };
        public override int[] reforgeValues => new[] { 1, 2, 3, 4, 6, 7 };
    }
    public class Sub_CritChance : Substat {
        public override string name => "Crit Chance";
        public Sub_CritChance(int val) : base(val) { }
        public override float scoreMulti => 8f / 5f;
        public override int[] maxRoll => new[] { 5, 5 };
        public override int[] minRoll => new[] { 3, 3 };
        public override int[] reforgeValues => new[] { 1, 2, 3, 4, 5, 6 };
    }
    public class Sub_FlatAttack : Substat {
        public override string name => "Attack";
        public Sub_FlatAttack(int val) : base(val) { }
        public override float scoreMulti => 3.46f / 39f;
        public override int[] maxRoll => new[] { 44, 46 };
        public override int[] minRoll => new[] { 31, 33 };
        public override int[] reforgeValues => new[] { 11, 22, 33, 44, 55, 66 };
    }
    public class Sub_FlatDefense : Substat {
        public override string name => "Defense";
        public Sub_FlatDefense(int val) : base(val) { }
        public override float scoreMulti => 4.99f / 31f;
        public override int[] maxRoll => new[] { 33, 35 };
        public override int[] minRoll => new[] { 26, 28 };
        public override int[] reforgeValues => new[] { 9, 18, 27, 36, 45, 54 };
    }
    public class Sub_FlatHealth : Substat {
        public override string name => "Health";
        public Sub_FlatHealth(int val) : base(val) { }
        public override float scoreMulti => 3.09f / 174f;
        public override int[] maxRoll => new[] { 192, 202 };
        public override int[] minRoll => new[] { 149, 157 };
        public override int[] reforgeValues => new[] { 56, 112, 168, 224, 280, 336 };
    }

}

namespace AntekaEquipmentAnalyzer {
    public class FlatHealth : Substat {
        public override string Name => "Health";
        public FlatHealth(int val) : base(val) { }
        public override float scoreMulti => 3.09f / 174f;
        public override int[] maxRoll => new[] { 192, 202 };
        public override int[] minRoll => new[] { 149, 157 };
        public override int[] reforgeValues => new[] { 56, 112, 168, 224, 280, 336 };
    }
}

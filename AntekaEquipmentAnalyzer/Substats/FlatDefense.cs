namespace AntekaEquipmentAnalyzer {
    public class FlatDefense : Substat {
        public override string name => "Defense";
        public FlatDefense(int val) : base(val) { }
        public override float scoreMulti => 4.99f / 31f;
        public override int[] maxRoll => new[] { 33, 35 };
        public override int[] minRoll => new[] { 26, 28 };
        public override int[] reforgeValues => new[] { 9, 18, 27, 36, 45, 54 };
    }
}

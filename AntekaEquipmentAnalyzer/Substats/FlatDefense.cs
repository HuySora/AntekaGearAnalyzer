namespace AntekaEquipmentAnalyzer {
    public class FlatDefense : Substat {
        public override string Name => "Defense";
        public FlatDefense(int val) : base(val) { }
        public override float ScoreMultiplier => 4.99f / 31f;
        public override int[] MaxRolls => new[] { 33, 35 };
        public override int[] MinRolls => new[] { 26, 28 };
        public override int[] ReforgeValues => new[] { 9, 18, 27, 36, 45, 54 };
    }
}

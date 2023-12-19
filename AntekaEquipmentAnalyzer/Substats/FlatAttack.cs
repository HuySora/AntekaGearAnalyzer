namespace AntekaEquipmentAnalyzer {
    public class FlatAttack : Substat {
        public override string Name => "Attack";
        public FlatAttack(int val) : base(val) { }
        public override float ScoreMultiplier => 3.46f / 39f;
        public override int[] MaxRolls => new[] { 44, 46 };
        public override int[] minRoll => new[] { 31, 33 };
        public override int[] reforgeValues => new[] { 11, 22, 33, 44, 55, 66 };
    }
}
